using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta los desembolsos del expediente. Fiel a VB6 sbDesembolsos_Load
        /// (frmPreaEstudiov2.frm línea ~17393): select * from CRD_PREA_DETALLE_DESEMBOLSOS
        /// where cod_PreAnalisis = &lt;expediente&gt;. La consulta anterior invocaba por error
        /// el SP de guardado (spCrdPreaGuardaDesembolsos), lo cual fallaba en tiempo de ejecución.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Consultar(
            int codEmpresa,
            string cod_preanalisis,
            string usuario)
        {
            var result = new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DesembolsosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                result.Result = ConsultarDesembolsosYBancos(connection, cod_preanalisis, usuario, out var sinBancos);

                // No se marca como error: replica el comportamiento de VB6, donde la
                // ausencia de bancos es una advertencia y no bloquea la carga del expediente.
                if (sinBancos)
                {
                    result.Description = "No existen Bancos [Creados/Asignados], verifique en Tesoreria.";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2DesembolsosResponse();
            }

            return result;
        }

        private static FrmPreaEstudiov2DesembolsosResponse ConsultarDesembolsosYBancos(
            IDbConnection connection,
            string cod_preanalisis,
            string usuario,
            out bool sinBancos)
        {
            var exp = cod_preanalisis.Trim().Replace("'", "''");

            var desembolsos = connection.Query<FrmPreaEstudiov2DesembolsoDto>(
                $"select IdX as id_desembolso, cod_Acredor as cod_acredor, Ordinario as ordinario," +
                " Descripcion as descripcion, Cuota as cuota, Monto as monto" +
                $" from CRD_PREA_DETALLE_DESEMBOLSOS where cod_PreAnalisis = '{exp}'",
                commandType: CommandType.Text
            ).ToList();

            var bancosParameters = new DynamicParameters();
            bancosParameters.Add("@Usuario", usuario?.Trim() ?? string.Empty, DbType.String);

            var bancos = connection.Query<FrmPreaEstudiov2DropdownDto>(
                "spCrd_SGT_Bancos_Desembolso",
                bancosParameters,
                commandType: CommandType.StoredProcedure
            ).ToList();

            sinBancos = bancos.Count == 0;

            return new FrmPreaEstudiov2DesembolsosResponse
            {
                desembolsos = desembolsos,
                bancos = bancos
            };
        }

        /// <summary>
        /// Guarda un desembolso del expediente. Fiel a VB6 sbDesembolso_Guardar
        /// (frmPreaEstudiov2.frm línea ~13148): exec spCrdPreaGuardaDesembolsos con 16
        /// parámetros posicionales, en el orden confirmado por el comentario de firma
        /// incluido en el propio código fuente VB6.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2DesembolsoGuardarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DesembolsosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var exp = request.cod_preanalisis.Trim().Replace("'", "''");
                var codAcreedor = (request.cod_acreedor ?? string.Empty).Trim().Replace("'", "''");
                var ordinario = request.ordinario ? 1 : 0;
                var descripcion = (request.descripcion ?? string.Empty).Trim().Replace("'", "''");
                var tipoGiro = (request.tipo_giro ?? string.Empty).Trim().Replace("'", "''");
                var cedulaDestino = (request.cedula_destino ?? string.Empty).Trim().Replace("'", "''");
                var cuenta = (request.cuenta ?? string.Empty).Trim().Replace("'", "''");
                var codDivisa = (request.cod_divisa ?? string.Empty).Trim().Replace("'", "''");
                var correo = (request.correo ?? string.Empty).Trim().Replace("'", "''");
                var detalle = (request.detalle ?? string.Empty).Trim().Replace("'", "''");
                var codBanco = (request.cod_banco ?? string.Empty).Trim().Replace("'", "''");

                var strSQL = "exec spCrdPreaGuardaDesembolsos " +
                    $"'{exp}', '{codAcreedor}', {ordinario}, '{descripcion}', {Dec(request.cuota)}, {Dec(request.monto)}, " +
                    $"'{tipoGiro}', '{cedulaDestino}', {request.tipo_cedula}, '{cuenta}', '{codDivisa}', '', " +
                    $"'{correo}', '{detalle}', '', {codBanco}";

                connection.Execute(strSQL, commandType: CommandType.Text);

                return Prea_frmPreaEstudiov2_Desembolsos_Consultar(codEmpresa, request.cod_preanalisis, request.usuario);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2DesembolsosResponse();
                return response;
            }
        }

        /// <summary>
        /// Elimina un desembolso del expediente. Fiel a VB6 sbDesembolso_Borrar
        /// (frmPreaEstudiov2.frm línea ~13169): exec spCrdPreaEliminarDesembolsos '&lt;expediente&gt;', &lt;idX&gt;.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Eliminar(
            int codEmpresa,
            string cod_preanalisis,
            int id_desembolso,
            string usuario)
        {
            var response = new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DesembolsosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var exp = cod_preanalisis.Trim().Replace("'", "''");
                var strSQL = $"exec spCrdPreaEliminarDesembolsos '{exp}', {id_desembolso}";
                connection.Execute(strSQL, commandType: CommandType.Text);

                return Prea_frmPreaEstudiov2_Desembolsos_Consultar(codEmpresa, cod_preanalisis, usuario);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2DesembolsosResponse();
                return response;
            }
        }
    }
}
