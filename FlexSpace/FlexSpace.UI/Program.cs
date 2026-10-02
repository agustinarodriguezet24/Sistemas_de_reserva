using System.Globalization;
using FlexSpace.BLL;

namespace FlexSpace.UI;

internal class Program
{
    private static ReservaService _reservas = null!;
    private static ClienteService _clientes = null!;

    private static void Main()
    {
        ServiceFactory.InicializarBaseDeDatos();
        _reservas = ServiceFactory.CrearReservaService();
        _clientes = ServiceFactory.CrearClienteService();

        bool salir = false;
        while (!salir)
        {
            Console.WriteLine();
            Console.WriteLine("===== FLEXSPACE =====");
            Console.WriteLine("1. Registrar nueva reserva");
            Console.WriteLine("2. Cancelar reserva");
            Console.WriteLine("3. Consultar reservas activas por puesto");
            Console.WriteLine("4. Listar clientes sancionados");
            Console.WriteLine("0. Salir");
            Console.Write("Opción: ");

            try
            {
                switch (Console.ReadLine()?.Trim())
                {
                    case "1": RegistrarReserva(); break;
                    case "2": CancelarReserva(); break;
                    case "3": ConsultarPorPuesto(); break;
                    case "4": ListarSancionados(); break;
                    case "0": salir = true; break;
                    default: Console.WriteLine("Opción inválida."); break;
                }
            }
            catch (ClienteSancionadoException ex)
            {
                Console.WriteLine($"[CLIENTE SANCIONADO] {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }
    }

    private static void RegistrarReserva()
    {
        int clienteId = LeerEntero("Id de cliente: ");
        int puestoId = LeerEntero("Id de puesto: ");
        DateTime inicio = LeerFecha("Inicio (dd/MM/yyyy HH:mm): ");
        DateTime fin = LeerFecha("Fin    (dd/MM/yyyy HH:mm): ");

        var p = _reservas.Cotizar(clienteId, puestoId, inicio, fin);
        var t = p.Tarifa;

        Console.WriteLine();
        Console.WriteLine("----- RESUMEN -----");
        Console.WriteLine($"Cliente: {p.Cliente.Nombre} ({p.Cliente.TipoCliente}, sanciones: {p.Cliente.SancionesActivas})");
        Console.WriteLine($"Puesto:  {p.Puesto.Codigo} - {p.Puesto.TipoPuesto}");
        Console.WriteLine($"Horario: {inicio:dd/MM/yyyy HH:mm} a {fin:dd/MM/yyyy HH:mm} ({t.Horas:0.##} hs)");
        Console.WriteLine($"Tarifa base/hora:        ${p.Puesto.TarifaBasePorHora:N2}");
        Console.WriteLine($"Subtotal:                ${t.Subtotal:N2}");
        if (t.RecargoFinDeSemana > 0) Console.WriteLine($"Recargo fin de semana:  +${t.RecargoFinDeSemana:N2}");
        if (t.DescuentoVolumen > 0)   Console.WriteLine($"Descuento por volumen:  -${t.DescuentoVolumen:N2}");
        if (t.DescuentoVip > 0)       Console.WriteLine($"Descuento VIP:          -${t.DescuentoVip:N2}");
        if (t.RecargoSancion > 0)     Console.WriteLine($"Recargo por sanciones:  +${t.RecargoSancion:N2} (sin descuentos)");
        Console.WriteLine($"TOTAL:                   ${t.Total:N2}");

        Console.Write("¿Confirmar reserva? (S/N): ");
        if (Console.ReadLine()?.Trim().ToUpper() == "S")
        {
            var reserva = _reservas.Registrar(clienteId, puestoId, inicio, fin);
            Console.WriteLine($"Reserva #{reserva.Id} confirmada.");
        }
        else
        {
            Console.WriteLine("Operación cancelada.");
        }
    }

    private static void CancelarReserva()
    {
        int id = LeerEntero("Id de reserva a cancelar: ");
        var res = _reservas.Cancelar(id);
        Console.WriteLine($"Reserva #{res.Reserva.Id} cancelada.");
        if (res.SancionAplicada)
            Console.WriteLine("Cancelación con menos de 2 horas de anticipación: se sumó 1 sanción al cliente.");
    }

    private static void ConsultarPorPuesto()
    {
        Console.Write("Código de puesto (ej: ESC-01): ");
        string codigo = Console.ReadLine() ?? "";
        var lista = _reservas.ListarFuturasPorPuesto(codigo);

        if (lista.Count == 0) { Console.WriteLine("No hay reservas futuras para ese puesto."); return; }

        Console.WriteLine($"{"Id",-5}{"Cliente",-9}{"Inicio",-18}{"Fin",-18}{"Costo",12}");
        foreach (var r in lista)
            Console.WriteLine($"{r.Id,-5}{r.ClienteId,-9}{r.FechaInicio.ToString("dd/MM/yyyy HH:mm"),-18}{r.FechaFin.ToString("dd/MM/yyyy HH:mm"),-18}{r.CostoTotal,12:N2}");
    }

    private static void ListarSancionados()
    {
        var lista = _clientes.ListarSancionados();
        if (lista.Count == 0) { Console.WriteLine("No hay clientes sancionados."); return; }

        Console.WriteLine($"{"Id",-5}{"Nombre",-20}{"Email",-22}{"Tipo",-10}{"Sanciones",-10}");
        foreach (var c in lista)
            Console.WriteLine($"{c.Id,-5}{c.Nombre,-20}{c.Email,-22}{c.TipoCliente,-10}{c.SancionesActivas,-10}");
    }

    // Solo captura de datos (sin reglas de negocio)
    private static int LeerEntero(string mensaje)
    {
        while (true)
        {
            Console.Write(mensaje);
            if (int.TryParse(Console.ReadLine(), out int valor)) return valor;
            Console.WriteLine("Ingrese un número entero válido.");
        }
    }

    private static DateTime LeerFecha(string mensaje)
    {
        while (true)
        {
            Console.Write(mensaje);
            if (DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                return fecha;
            Console.WriteLine("Formato inválido. Use dd/MM/yyyy HH:mm");
        }
    }
}
