using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.Security;
using Microsoft.Extensions.Configuration;

namespace Galileo.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasParametrosDB
    {
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 5;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCajasParametrosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
            _Security_MainDB = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de parámetros de cajas visibles.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        public ErrorDto<List<CajasParametrosData>> Cajas_Parametros_Lista_Obtener(int CodEmpresa,FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<CajasParametrosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasParametrosData>()
            };

            try
            {
                filtros ??= new FiltrosLazyLoadData();
                using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                cn.Execute("spCajas_Parametros", commandType: CommandType.StoredProcedure);

                var parameters = new DynamicParameters();
                var where = " WHERE visible = 'S' ";

                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    where += @"
                AND (
                       RTRIM(cod_parametro) LIKE @filtro
                    OR RTRIM(descripcion)   LIKE @filtro
                    OR RTRIM(valor)         LIKE @filtro
                )";

                    parameters.Add("@filtro", $"%{filtros.filtro}%");
                }

                var allowedSortFields = new[]
                {
                    "cod_parametro",
                    "descripcion",
                    "valor"
                };

                var sortField = allowedSortFields
                    .FirstOrDefault(f =>
                        f.Equals(filtros.sortField ?? string.Empty,
                                 StringComparison.OrdinalIgnoreCase))
                    ?? "cod_parametro";

                var sortDirection = filtros.sortOrder == 0 ? "DESC" : "ASC";

                var q = $@"
            SELECT 
                  RTRIM(cod_parametro) AS cod_parametro
                , RTRIM(descripcion)   AS descripcion
                , RTRIM(valor)         AS valor
                , tipo
                , visible
                , notas
                , Inicio_Fecha     AS inicio_fecha
                , Modifica_Fecha   AS modifica_fecha
                , Modifica_Usuario AS modifica_usuario
            FROM CAJAS_PARAMETROS
            {where}
            ORDER BY {sortField} {sortDirection};";

                result.Result = cn.Query<CajasParametrosData>(q, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }


        /// <summary>
        /// Actualiza el valor de un parámetro de cajas.
        /// <param name="CodEmpresa"></param>
        /// <param name="parametro"></param>
        /// </summary>

        public ErrorDto Cajas_Parametros_Guardar(int CodEmpresa, CajasParametrosData parametro)
        {
            using var cn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {

                if (string.IsNullOrWhiteSpace(parametro.cod_parametro))
                {
                    resp.Code = -2;
                    resp.Description = "Debe indicar el código de parámetro.";
                    return resp;
                }

                var q = @"
            UPDATE CAJAS_PARAMETROS
            SET  valor            = @valor,
                 Modifica_Fecha   = dbo.MyGetdate(),
                 Modifica_Usuario = @modifica_usuario
            WHERE cod_parametro   = @cod_parametro;";

                var rows = cn.Execute(q, new
                {
                    cod_parametro = parametro.cod_parametro,
                    valor = parametro.valor ?? string.Empty,
                    modifica_usuario = parametro.modifica_usuario ?? string.Empty
                });

                if (rows == 0)
                {
                    resp.Code = -2;
                    resp.Description = $"El parámetro {parametro.cod_parametro} no existe.";
                    return resp;
                }
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = parametro.modifica_usuario ?? string.Empty,
                    DetalleMovimiento = $"Parámetro de Cajas: {parametro.cod_parametro}  Valor: {parametro.valor}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }


    }
}
