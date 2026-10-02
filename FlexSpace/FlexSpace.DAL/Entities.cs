namespace FlexSpace.DAL.Entities;

public enum TipoCliente { Estandar, VIP }
public enum TipoPuesto { EscritorioIndividual, SalaReuniones, CabinaPrivada }
public enum EstadoReserva { Confirmada, Cancelada, Finalizada }

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public TipoCliente TipoCliente { get; set; }
    public int SancionesActivas { get; set; }
}

public class Puesto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public TipoPuesto TipoPuesto { get; set; }
    public decimal TarifaBasePorHora { get; set; }
}

public class Reserva
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int PuestoId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public EstadoReserva Estado { get; set; }
    public decimal CostoTotal { get; set; }
}
