using System.Globalization;
using Microsoft.Data.Sqlite;

namespace FlexSpace.DAL;

public static class Db
{
    private const string ConnectionString = "Data Source=flexspace.db";
    private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

    internal static SqliteConnection Abrir()
    {
        var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        return conn;
    }

    internal static string Fmt(DateTime f) => f.ToString(FormatoFecha, CultureInfo.InvariantCulture);
    internal static DateTime Parse(string s) => DateTime.ParseExact(s, FormatoFecha, CultureInfo.InvariantCulture);

    public static void Inicializar()
    {
        using var conn = Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Cliente (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Email TEXT NOT NULL,
    TipoCliente TEXT NOT NULL,
    SancionesActivas INTEGER NOT NULL DEFAULT 0
);
CREATE TABLE IF NOT EXISTS Puesto (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Codigo TEXT NOT NULL UNIQUE,
    TipoPuesto TEXT NOT NULL,
    TarifaBasePorHora REAL NOT NULL
);
CREATE TABLE IF NOT EXISTS Reserva (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ClienteId INTEGER NOT NULL,
    PuestoId INTEGER NOT NULL,
    FechaInicio TEXT NOT NULL,
    FechaFin TEXT NOT NULL,
    Estado TEXT NOT NULL,
    CostoTotal REAL NOT NULL,
    FOREIGN KEY (ClienteId) REFERENCES Cliente(Id),
    FOREIGN KEY (PuestoId) REFERENCES Puesto(Id)
);

INSERT OR IGNORE INTO Cliente (Id, Nombre, Email, TipoCliente, SancionesActivas) VALUES
 (1, 'Ana Pérez',   'ana@mail.com',   'Estandar', 0),
 (2, 'Bruno Gómez', 'bruno@mail.com', 'VIP',      0),
 (3, 'Carla Díaz',  'carla@mail.com', 'Estandar', 1),
 (4, 'Diego Ruiz',  'diego@mail.com', 'Estandar', 3);

INSERT OR IGNORE INTO Puesto (Id, Codigo, TipoPuesto, TarifaBasePorHora) VALUES
 (1, 'ESC-01',  'EscritorioIndividual', 1500),
 (2, 'SALA-01', 'SalaReuniones',        5000),
 (3, 'CAB-01',  'CabinaPrivada',        3000);";
        cmd.ExecuteNonQuery();
    }
}
