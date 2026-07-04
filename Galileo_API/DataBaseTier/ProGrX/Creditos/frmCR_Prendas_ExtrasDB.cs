using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPrendasExtrasDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;

        private const int ModuloCreditos = 3;
        private const string MensajeOk = "Ok";
        private const string GuardadoCorrectamente = "Informacion guardada satisfactoriamente...";

        public FrmCrPrendasExtrasDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrPrendasExtrasDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }

        /// <summary>
        /// Consulta el encabezado y la lista de extras de una prenda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="prendaId"></param>
        /// <returns></returns>
        public ErrorDto<CrPrendasExtrasConsultaData> CR_Prendas_Extras_Consulta(int codEmpresa, long prendaId)
        {
            if (prendaId <= 0)
            {
                return DbHelper.CreateErrorResponse<CrPrendasExtrasConsultaData>(
                    "Debe indicar la prenda.",
                    -1,
                    new CrPrendasExtrasConsultaData());
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var result = new CrPrendasExtrasConsultaData
                {
                    encabezado = ConsultarEncabezado(conn, prendaId),
                    extras = ConsultarExtras(conn, prendaId)
                };
                result.total_monto = result.extras.Sum(item => item.monto_extras);

                return new ErrorDto<CrPrendasExtrasConsultaData>
                {
                    Code = 0,
                    Description = MensajeOk,
                    Result = result
                };
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPrendasExtrasConsultaData>(
                    ex.Message ?? string.Empty,
                    -1,
                    new CrPrendasExtrasConsultaData());
            }
        }

        /// <summary>
        /// Guarda los montos de extras asociados a una prenda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrPrendasExtrasGuardarData> CR_Prendas_Extras_Guardar(
            int codEmpresa,
            CrPrendasExtrasGuardarRequest request)
        {
            if (request.prenda_id <= 0)
            {
                return DbHelper.CreateErrorResponse<CrPrendasExtrasGuardarData>(
                    "Debe indicar la prenda.",
                    -1,
                    new CrPrendasExtrasGuardarData());
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                foreach (var extra in request.extras)
                {
                    var result = GuardarExtra(conn, request.prenda_id, extra, request.usuario);
                    RegistrarBitacora(codEmpresa, request.usuario, result);
                }

                return new ErrorDto<CrPrendasExtrasGuardarData>
                {
                    Code = 0,
                    Description = GuardadoCorrectamente,
                    Result = new CrPrendasExtrasGuardarData
                    {
                        total_monto = request.extras.Sum(item => item.monto_extras)
                    }
                };
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPrendasExtrasGuardarData>(
                    ex.Message ?? string.Empty,
                    -1,
                    new CrPrendasExtrasGuardarData());
            }
        }

        private static CrPrendasExtrasEncabezadoData ConsultarEncabezado(SqlConnection conn, long prendaId)
        {
            const string SqlEncabezado = "exec spCrd_Prenda_Consulta_Lite @PrendaId;";

            return conn.QueryFirstOrDefault<CrPrendasExtrasEncabezadoData>(
                SqlEncabezado,
                new { PrendaId = prendaId }) ?? new CrPrendasExtrasEncabezadoData();
        }

        private static List<CrPrendasExtrasData> ConsultarExtras(SqlConnection conn, long prendaId)
        {
            const string SqlExtras = "exec spCrd_Prenda_Consulta_Extras @PrendaId;";

            return conn.Query<CrPrendasExtrasData>(
                SqlExtras,
                new { PrendaId = prendaId }).ToList();
        }

        private static CrPrendasExtrasSpResult GuardarExtra(
            SqlConnection conn,
            long prendaId,
            CrPrendasExtrasData extra,
            string usuario)
        {
            const string SqlGuardar = @"
            exec spCrd_Prenda_Consulta_Extras_Add
                @PrendaId,
                @ExtraId,
                @Monto,
                @Usuario;";

            return conn.QueryFirstOrDefault<CrPrendasExtrasSpResult>(
                SqlGuardar,
                new
                {
                    PrendaId = prendaId,
                    ExtraId = extra.id_extra,
                    Monto = extra.monto_extras,
                    Usuario = (usuario ?? string.Empty).Trim()
                }) ?? new CrPrendasExtrasSpResult();
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, CrPrendasExtrasSpResult result)
        {
            if (result.pass != 1 || string.Equals(result.movimiento, "NA", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).Trim(),
                Movimiento = result.movimiento,
                DetalleMovimiento = result.mensaje,
                Modulo = ModuloCreditos
            });
        }
    }
}
