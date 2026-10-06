using FlexSpace.DAL.Entities;
using Microsoft.Data.Sqlite;

namespace FlexSpace.DAL;

public class PuestoRepository
{
    public Puesto? ObtenerPorId(int id)
    {
        using var conn = Db.Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Codigo, TipoPuesto, TarifaBasePorHora FROM Puesto WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    public Puesto? ObtenerPorCodigo(string codigo)
    {
        using var conn = Db.Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Codigo, TipoPuesto, TarifaBasePorHora FROM Puesto WHERE Codigo = @c";
        cmd.Parameters.AddWithValue("@c", codigo);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Mapear(r) : null;
    }

    private static Puesto Mapear(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Codigo = r.GetString(1),
        TipoPuesto = Enum.Parse<TipoPuesto>(r.GetString(2)),
        TarifaBasePorHora = Convert.ToDecimal(r.GetDouble(3))
    };
}
