using FlexSpace.DAL;
using FlexSpace.DAL.Entities;

namespace FlexSpace.BLL;

public class PresupuestoReserva
{
    public Cliente Cliente { get; set; } = null!;
    public Puesto Puesto { get; set; } = null!;
    public DateTime Inicio { get; set; }
    public DateTime Fin { get; set; }
    public ResultadoTarifa Tarifa { get; set; } = null!;
}

public record ResultadoCancelacion(Reserva Reserva, bool SancionAplicada);

public class ReservaService
{
    private const int MaxSanciones = 3;
    private readonly ReservaRepository _reservas;
    private readonly ClienteRepository _clientes;
    private readonly PuestoRepository _puestos;

    public ReservaService(ReservaRepository reservas, ClienteRepository clientes, PuestoRepository puestos)
    {
        _reservas = reservas;
        _clientes = clientes;
        _puestos = puestos;
    }

    // Valida todo y calcula el precio, sin guardar nada
    public PresupuestoReserva Cotizar(int clienteId, int puestoId, DateTime inicio, DateTime fin)
    {
        if (fin <= inicio)
            throw new ArgumentException("La fecha de fin debe ser posterior a la de inicio.");
        if (inicio < DateTime.Now)
            throw new ArgumentException("No se pueden crear reservas en el pasado.");

        var cliente = _clientes.ObtenerPorId(clienteId)
            ?? throw new InvalidOperationException($"No existe el cliente con Id {clienteId}.");

        if (cliente.SancionesActivas >= MaxSanciones)
            throw new ClienteSancionadoException(
                $"El cliente {cliente.Nombre} tiene {cliente.SancionesActivas} sanciones activas y no puede reservar.");

        var puesto = _puestos.ObtenerPorId(puestoId)
            ?? throw new InvalidOperationException($"No existe el puesto con Id {puestoId}.");

        if (_reservas.ExisteSolapamiento(puestoId, inicio, fin))
            throw new InvalidOperationException("El puesto ya tiene una reserva confirmada en ese horario.");

        var tarifa = FabricaTarifa.Crear(cliente).Calcular(puesto.TarifaBasePorHora, inicio, fin);

        return new PresupuestoReserva
        {
            Cliente = cliente,
            Puesto = puesto,
            Inicio = inicio,
            Fin = fin,
            Tarifa = tarifa
        };
    }

    // Vuelve a validar y recién ahí persiste
    public Reserva Registrar(int clienteId, int puestoId, DateTime inicio, DateTime fin)
    {
        var p = Cotizar(clienteId, puestoId, inicio, fin);

        var reserva = new Reserva
        {
            ClienteId = p.Cliente.Id,
            PuestoId = p.Puesto.Id,
            FechaInicio = inicio,
            FechaFin = fin,
            Estado = EstadoReserva.Confirmada,
            CostoTotal = p.Tarifa.Total
        };
        reserva.Id = _reservas.Insertar(reserva);
        return reserva;
    }

    public ResultadoCancelacion Cancelar(int reservaId)
    {
        var reserva = _reservas.ObtenerPorId(reservaId)
            ?? throw new InvalidOperationException($"No existe la reserva con Id {reservaId}.");

        if (reserva.Estado != EstadoReserva.Confirmada)
            throw new InvalidOperationException("Solo se pueden cancelar reservas confirmadas.");

        bool tardia = reserva.FechaInicio - DateTime.Now < TimeSpan.FromHours(2);

        _reservas.Cancelar(reservaId);
        if (tardia) _clientes.IncrementarSanciones(reserva.ClienteId);

        return new ResultadoCancelacion(reserva, tardia);
    }

    public List<Reserva> ListarFuturasPorPuesto(string codigoPuesto)
    {
        if (string.IsNullOrWhiteSpace(codigoPuesto))
            throw new ArgumentException("Debe indicar un código de puesto.");

        if (_puestos.ObtenerPorCodigo(codigoPuesto.Trim()) == null)
            throw new InvalidOperationException($"No existe el puesto con código {codigoPuesto}.");

        return _reservas.ListarFuturasPorPuesto(codigoPuesto.Trim(), DateTime.Now);
    }
}
