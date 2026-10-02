using FlexSpace.DAL.Entities;

namespace FlexSpace.BLL;

public class ResultadoTarifa
{
    public decimal Horas { get; set; }
    public decimal Subtotal { get; set; }
    public decimal RecargoFinDeSemana { get; set; }
    public decimal DescuentoVolumen { get; set; }
    public decimal DescuentoVip { get; set; }
    public decimal RecargoSancion { get; set; }
    public decimal Total { get; set; }
}

public interface IEstrategiaTarifa
{
    ResultadoTarifa Calcular(decimal tarifaBase, DateTime inicio, DateTime fin);
}

// Cliente estándar: fin de semana (+15%) y volumen (-10%)
public class EstrategiaEstandar : IEstrategiaTarifa
{
    public virtual ResultadoTarifa Calcular(decimal tarifaBase, DateTime inicio, DateTime fin)
    {
        var r = CrearBase(tarifaBase, inicio, fin);
        decimal acumulado = r.Subtotal;

        if (IncluyeFinDeSemana(inicio, fin))
        {
            r.RecargoFinDeSemana = r.Subtotal * 0.15m;
            acumulado += r.RecargoFinDeSemana;
        }

        if (r.Horas >= 5)
        {
            r.DescuentoVolumen = acumulado * 0.10m;
            acumulado -= r.DescuentoVolumen;
        }

        acumulado = AplicarBeneficioCliente(r, acumulado);
        r.Total = Math.Round(acumulado, 2);
        return r;
    }

    protected virtual decimal AplicarBeneficioCliente(ResultadoTarifa r, decimal acumulado) => acumulado;

    protected static ResultadoTarifa CrearBase(decimal tarifaBase, DateTime inicio, DateTime fin)
    {
        var horas = (decimal)(fin - inicio).TotalHours;
        return new ResultadoTarifa { Horas = horas, Subtotal = horas * tarifaBase };
    }

    protected static bool IncluyeFinDeSemana(DateTime inicio, DateTime fin)
    {
        for (var dia = inicio.Date; dia < fin; dia = dia.AddDays(1))
            if (dia.DayOfWeek == DayOfWeek.Saturday || dia.DayOfWeek == DayOfWeek.Sunday)
                return true;
        return false;
    }
}

// Cliente VIP: igual que estándar + 5% de descuento final
public class EstrategiaVip : EstrategiaEstandar
{
    protected override decimal AplicarBeneficioCliente(ResultadoTarifa r, decimal acumulado)
    {
        r.DescuentoVip = acumulado * 0.05m;
        return acumulado - r.DescuentoVip;
    }
}

// Cliente con sanciones: pierde todos los descuentos y paga +20% sobre la base
public class EstrategiaSancionado : EstrategiaEstandar
{
    public override ResultadoTarifa Calcular(decimal tarifaBase, DateTime inicio, DateTime fin)
    {
        var r = CrearBase(tarifaBase, inicio, fin);
        decimal acumulado = r.Subtotal;

        if (IncluyeFinDeSemana(inicio, fin))
        {
            r.RecargoFinDeSemana = r.Subtotal * 0.15m;
            acumulado += r.RecargoFinDeSemana;
        }

        r.RecargoSancion = r.Subtotal * 0.20m;
        acumulado += r.RecargoSancion;

        r.Total = Math.Round(acumulado, 2);
        return r;
    }
}

public static class FabricaTarifa
{
    public static IEstrategiaTarifa Crear(Cliente cliente)
    {
        if (cliente.SancionesActivas > 0) return new EstrategiaSancionado();
        if (cliente.TipoCliente == TipoCliente.VIP) return new EstrategiaVip();
        return new EstrategiaEstandar();
    }
}
