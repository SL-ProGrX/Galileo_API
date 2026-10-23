using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier;

public sealed class FrmUsDepartamentosDb(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnString")
        ?? throw new InvalidOperationException("No está configurada la conexión del portal.");

    public List<Departamento> ObtenerTodos(string? codigoActual, bool soloActivos)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = """
            SELECT COD_DEPARTAMENTO, DESCRIPCION, ACTIVO, REGISTRO_FECHA,
                   REGISTRO_USUARIO, MODIFICA_FECHA, MODIFICA_USUARIO
            FROM dbo.US_DEPARTAMENTOS
            WHERE @SoloActivos = 0 OR ACTIVO = 1 OR COD_DEPARTAMENTO = @CodigoActual
            ORDER BY DESCRIPCION, COD_DEPARTAMENTO;
            """;
        return connection.Query<Departamento>(sql, new { SoloActivos = soloActivos, CodigoActual = codigoActual }).ToList();
    }

    public ErrorDto Guardar(DepartamentoRequest request, string actor, bool insertar)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        const string existsSql = "SELECT COUNT(*) FROM dbo.US_DEPARTAMENTOS WITH (UPDLOCK, HOLDLOCK) WHERE COD_DEPARTAMENTO = @Codigo;";
        var exists = connection.ExecuteScalar<int>(existsSql, new { Codigo = request.Cod_Departamento }, transaction) != 0;
        if (exists == insertar)
            return DbHelper.ErrorResponse(insertar ? "Ya existe un departamento con ese código." : "El departamento ya no existe. Actualice la lista.");

        const string insertSql = """
            INSERT INTO dbo.US_DEPARTAMENTOS
                (COD_DEPARTAMENTO, DESCRIPCION, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO)
            VALUES (@Codigo, @Descripcion, @Activo, GETDATE(), @Actor);
            """;
        const string updateSql = """
            UPDATE dbo.US_DEPARTAMENTOS
            SET DESCRIPCION = @Descripcion, ACTIVO = @Activo,
                MODIFICA_FECHA = GETDATE(), MODIFICA_USUARIO = @Actor
            WHERE COD_DEPARTAMENTO = @Codigo;
            """;
        connection.Execute(insertar ? insertSql : updateSql,
            new { Codigo = request.Cod_Departamento, request.Descripcion, request.Activo, Actor = actor }, transaction);
        transaction.Commit();
        return DbHelper.CreateOkResponse();
    }

    public ErrorDto Eliminar(string codigo)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        const string referencesSql = "SELECT COUNT(*) FROM dbo.US_USUARIOS WITH (UPDLOCK, HOLDLOCK) WHERE COD_DEPARTAMENTO = @Codigo;";
        if (connection.ExecuteScalar<int>(referencesSql, new { Codigo = codigo }, transaction) > 0)
            return DbHelper.ErrorResponse("El departamento está asignado a usuarios. Puede inactivarlo para evitar nuevas asignaciones.");

        const string deleteSql = "DELETE FROM dbo.US_DEPARTAMENTOS WHERE COD_DEPARTAMENTO = @Codigo;";
        if (connection.Execute(deleteSql, new { Codigo = codigo }, transaction) == 0)
            return DbHelper.ErrorResponse("El departamento ya no existe. Actualice la lista.");
        transaction.Commit();
        return DbHelper.CreateOkResponse();
    }
}
