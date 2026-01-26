using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier
{
    public class FrmTesTePlanesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;

        public FrmTesTePlanesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            DBBitacora = new MSecurityMainDb(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data) => DBBitacora.Bitacora(data);

        // =========================
        // Helpers (reducen duplicación)
        // =========================

        private ErrorDto<T> WithConn<T>(int codEmpresa, Func<IDbConnection, T> work)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            try
            {
                return DbHelper.CreateOkResponse(work(conn));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<T>(ex.Message);
            }
        }

        private ErrorDto WithConn(int codEmpresa, Action<IDbConnection> work, string okMessage)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            try
            {
                work(conn);
                return DbHelper.OkResponse(okMessage);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static TesBancoPlanesData ParsePlan(string infoPlan)
            => JsonConvert.DeserializeObject<TesBancoPlanesData>(infoPlan) ?? new TesBancoPlanesData();

        private void RegistrarBitacoraPlan(int codEmpresa, TesBancoPlanesData request, string movimiento)
        {
            var usuario = (request.registro_usuario ?? string.Empty).ToUpperInvariant();

            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento =
                    $"Cta Id: {request.id_banco}, Plan: {request.cod_plan}, Consec Id: {request.numero_te}, Consec Interno: {request.numero_interno}",
                Movimiento = movimiento,
                Modulo = 9
            });
        }

        private void EjecutarRegistroPlan(IDbConnection conn, TesBancoPlanesData request, string accion)
        {
            const string sql = @"
EXEC spTes_Planes_Registro
    @banco, @codPlan, @consecId, @consecInt, @usuario, @accion;";

            conn.Execute(sql, new
            {
                banco = request.id_banco,
                codPlan = request.cod_plan,
                consecId = request.numero_te,
                consecInt = request.numero_interno,
                usuario = (request.registro_usuario ?? string.Empty).ToUpperInvariant(),
                accion
            });
        }

        // =========================
        // Públicos
        // =========================

        /// <summary>
        /// Hacer scroll entre los planes
        /// </summary>
        public ErrorDto<TesBancoPlanesData> TES_Planes_Scroll(int CodEmpresa, int scrollCode, string codPlan, int banco)
        {
            return WithConn(CodEmpresa, conn =>
            {
                const string NextSql = @"
SELECT TOP (1) *
FROM TES_BANCO_PLANES_TE
WHERE id_banco = @banco AND cod_Plan > @codPlan
ORDER BY cod_Plan ASC;";

                const string PrevSql = @"
SELECT TOP (1) *
FROM TES_BANCO_PLANES_TE
WHERE id_banco = @banco AND cod_Plan < @codPlan
ORDER BY cod_Plan DESC;";

                var sql = scrollCode == 1 ? NextSql : PrevSql;

                return conn.QueryFirstOrDefault<TesBancoPlanesData>(sql, new { banco, codPlan })
                       ?? new TesBancoPlanesData();
            });
        }

        /// <summary>
        /// Obtener información de planes
        /// </summary>
        public ErrorDto<TesBancoPlanesData> TES_PlanesConsulta_Obtener(int CodEmpresa, int banco, string codPlan)
        {
            return WithConn(CodEmpresa, conn =>
            {
                const string sql = @"EXEC spTes_Planes_Consulta @banco, @codPlan;";
                return conn.QueryFirstOrDefault<TesBancoPlanesData>(sql, new { banco, codPlan })
                       ?? new TesBancoPlanesData();
            });
        }

        /// <summary>
        /// Obtener información de grupos bancarios
        /// </summary>
        public ErrorDto<Galileo.Models.ProGrX.Bancos.TesBancosGruposData> TES_Planes_BancosGrupos_Obtener(int CodEmpresa, int banco)
        {
            return WithConn(CodEmpresa, conn =>
            {
                const string sql = @"
SELECT B.ID_BANCO, B.COD_GRUPO, B.DESCRIPCION, B.DESC_CORTA,
       Bg.DESCRIPCION AS Banco_Desc, Bg.DESC_CORTA AS Banco_Desc_Corta
FROM TES_BANCOS B
INNER JOIN TES_BANCOS_GRUPOS Bg ON B.COD_GRUPO = Bg.COD_GRUPO
WHERE B.ID_Banco = @banco;";

                return conn.QueryFirstOrDefault<Galileo.Models.ProGrX.Bancos.TesBancosGruposData>(sql, new { banco })
                       ?? new Galileo.Models.ProGrX.Bancos.TesBancosGruposData();
            });
        }

        /// <summary>
        /// Agregar o actualizar un plan
        /// </summary>
        public ErrorDto TES_Planes_Guardar(int CodEmpresa, string infoPlan)
        {
            var request = ParsePlan(infoPlan);

            return WithConn(CodEmpresa, conn =>
            {
                EjecutarRegistroPlan(conn, request, "A");
                RegistrarBitacoraPlan(CodEmpresa, request, "REGISTRA - WEB");
            }, "Plan Registrado Satisfactoriamente!");
        }

        /// <summary>
        /// Borrar un plan
        /// </summary>
        public ErrorDto TES_Planes_Borrar(int CodEmpresa, string infoPlan)
        {
            var request = ParsePlan(infoPlan);

            return WithConn(CodEmpresa, conn =>
            {
                EjecutarRegistroPlan(conn, request, "E");
                RegistrarBitacoraPlan(CodEmpresa, request, "ELIMINA - WEB");
            }, "Plan Eliminado Satisfactoriamente!");
        }
    }
}
