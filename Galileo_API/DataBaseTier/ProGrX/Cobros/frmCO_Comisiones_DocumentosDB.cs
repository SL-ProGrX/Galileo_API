using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoComisionesDocumentosDB
    {

        private readonly IConfiguration _config;

        public FrmCoComisionesDocumentosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Consulta el listado de tipos de comisiones de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoComisionesDocumentosData>> CO_ComisionesDocumento_Obtener(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        Ant.TIPO_DOCUMENTO,
                        Ant.DESCRIPCION,
                        ISNULL(Asg.TIPO_DOCUMENTO,'No-ASG') AS Asignado,
                        Asg.Registro_Fecha,
                        Asg.Registro_Usuario
                    FROM dbo.SIF_DOCUMENTOS Ant
                    LEFT JOIN dbo.CBR_COMISIONES_TDOC Asg
                        ON Ant.TIPO_DOCUMENTO = Asg.TIPO_DOCUMENTO
                    ORDER BY ISNULL(Asg.TIPO_DOCUMENTO,'000') DESC,
                             Ant.TIPO_DOCUMENTO;";

            return DbHelper.ExecuteListQuery<CoComisionesDocumentosData>(new PortalDB(_config), CodEmpresa, query);
        }

        /// <summary>
        /// Activa un tipo de comision 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo_documento"></param>
        /// <returns></returns>
        public ErrorDto CO_ComisionesDocumento_Insertar(int CodEmpresa, string usuario, string tipo_documento)
        {
            var tipoDocumento = NormalizarTipoDocumento(tipo_documento);

            if (string.IsNullOrWhiteSpace(tipoDocumento))
            {
                return DbHelper.ErrorResponse("El tipo de documento es requerido.", -2);
            }

            const string query = @"
                    INSERT INTO dbo.CBR_COMISIONES_TDOC
                    (
                        TIPO_DOCUMENTO,
                        registro_fecha,
                        registro_usuario
                    )
                    VALUES
                    (
                        @tipoDocumento,
                        dbo.mygetdate(),
                        @usuario
                    );";

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new { tipoDocumento, usuario });

            return result;
        }

        /// <summary>
        /// Elimina la activacion de un tipo de comision
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="tipo_documento"></param>
        /// <returns></returns>
        public ErrorDto CO_ComisionesDocumento_Delete(int CodEmpresa, string Usuario, string tipo_documento)
        {
            var tipoDocumento = NormalizarTipoDocumento(tipo_documento);

            if (string.IsNullOrWhiteSpace(tipoDocumento))
            {
                return DbHelper.ErrorResponse("El tipo de documento es requerido.", -2);
            }

            const string query = @"DELETE FROM dbo.CBR_COMISIONES_TDOC WHERE TIPO_DOCUMENTO = @tipoDocumento;";

            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new { tipoDocumento });
        }

        private static string NormalizarTipoDocumento(string? tipoDocumento)
        {
            return (tipoDocumento ?? string.Empty).Trim();
        }
    }
}