using FlexSpace.DAL;
using FlexSpace.DAL.Entities;

namespace FlexSpace.BLL;

public class ClienteService
{
    private readonly ClienteRepository _clientes;

    public ClienteService(ClienteRepository clientes) => _clientes = clientes;

    public List<Cliente> ListarSancionados() => _clientes.ListarSancionados();
}
