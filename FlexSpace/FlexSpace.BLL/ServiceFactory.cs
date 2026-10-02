using FlexSpace.DAL;

namespace FlexSpace.BLL;

public static class ServiceFactory
{
    public static void InicializarBaseDeDatos() => Db.Inicializar();

    public static ReservaService CrearReservaService() =>
        new(new ReservaRepository(), new ClienteRepository(), new PuestoRepository());

    public static ClienteService CrearClienteService() =>
        new(new ClienteRepository());
}
