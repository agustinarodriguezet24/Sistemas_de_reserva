using FlexSpace.DAL.Entities;
using Microsoft.Data.Sqlite;

namespace FlexSpace.DAL;

public class ReservaRepository
{
    private const string Columnas = "r.Id, r.ClienteId, r.PuestoId, r.FechaInicio, r.FechaFin, r.Estado, r.CostoTotal";

    public bool ExisteSolapamiento(int puestoId, DateTime inicio, DateTime fin)
    {
        using var conn = Db.Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT COUNT(1) FROM Reserva
                            WHERE PuestoId = @p AND Estado = 'Confirmada'
                              AND FechaInicio < @fin AND FechaFin > @ini";
        cmd.Parameters.AddWithValue("@p", puestoId);
        cmd.Parameters.AddWithValue("@ini", Db.Fmt(inicio));
        cmd.Parameters.AddWithValue("@fin", Db.Fmt(fin));
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public int Insertar(Reserva reserva)
    {
        using var conn = Db.Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Reserva (ClienteId, PuestoId, FechaInicio, FechaFin, Estado, CostoTotal)
                            VALUES (@c, @p, @ini, @fin, @est, @costo);
                            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@c", reserva.ClienteId);
        cmd.Parameters.AddWithValue("@p", reserva.PuestoId);
        cmd.Parameters.AddWithValue("@ini", Db.Fmt(reserva.FechaInicio));
        cmd.Parameters.AddWithValue("@fin", Db.Fmt(reserva.FechaFin));
        cmd.Parameters.AddWithValue("@est", reserva.Estado.ToString());
        cmd.Parameters.AddWithValue("@costo", (double)reserva.CostoTotal);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public Reserva? ObtenerPorId(int id)
    {
        using var conn = Db.Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {Columnas} FROM Reserva r WHERE r.Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var rd = cmd.ExecuteReader();
        return rd.Read() ? Mapear(rd) : null;
    }

    public void Cancelar(int id)
    {
        using var conn = Db.Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Reserva SET Estado = 'Cancelada' WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public List<Reserva> ListarFuturasPorPuesto(string codigoPuesto, DateTime ahora)
    {
        var lista = new List<Reserva>();
        using var conn = Db.Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"SELECT {Columnas} FROM Reserva r
                             INNER JOIN Puesto p ON p.Id = r.PuestoId
                             WHERE p.Codigo = @cod AND r.Estado = 'Confirmada' AND r.FechaInicio > @ahora
                             ORDER BY r.FechaInicio";
        cmd.Parameters.AddWithValue("@cod", codigoPuesto);
        cmd.Parameters.AddWithValue("@ahora", Db.Fmt(ahora));
        using var rd = cmd.ExecuteReader();
        while (rd.Read()) lista.Add(Mapear(rd));
        return lista;
    }

    private static Reserva Mapear(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        ClienteId = r.GetInt32(1),
        PuestoId = r.GetInt32(2),
        FechaInicio = Db.Parse(r.GetString(3)),
        FechaFin = Db.Parse(r.GetString(4)),
        Estado = Enum.Parse<EstadoReserva>(r.GetString(5)),
        CostoTotal = Convert.ToDecimal(r.GetDouble(6))
    };
}
