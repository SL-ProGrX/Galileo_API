using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSeguimientoEtiquetasDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrSeguimientoEtiquetasDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<List<CrSeguimientoEtiquetasData>> Cr_SeguimientoEtiquetas_Lista_Obtener(int codEmpresa, int idSolicitud)
        {
            if (idSolicitud <= 0)
            {
                return new ErrorDto<List<CrSeguimientoEtiquetasData>>
                {
                    Code = -1,
                    Description = "Debe indicar la solicitud.",
                    Result = []
                };
            }

            const string sqlQuery = @"
                SELECT
                    O.LINEA AS linea,
                    RTRIM(O.CODIGO) AS codigo,
                    O.ID_SOLICITUD AS id_solicitud,
                    RTRIM(O.TAG_CODIGO) AS tag_codigo,
                    ISNULL(RTRIM(O.ASIGNADO_A), '') AS asignado_a,
                    O.REGISTRO_FECHA AS registro_fecha,
                    ISNULL(RTRIM(O.REGISTRO_USUARIO), '') AS registro_usuario,
                    ISNULL(RTRIM(O.NOTAS), '') AS notas,
                    ISNULL(RTRIM(T.DESCRIPCION), '') AS etiqueta
                FROM CRD_OPERACION_TAGS O
                INNER JOIN CRD_TAGS T
                    ON O.TAG_CODIGO = T.TAG_CODIGO
                WHERE O.ID_SOLICITUD = @IdSolicitud
                ORDER BY O.REGISTRO_FECHA;";

            return DbHelper.ExecuteListQuery<CrSeguimientoEtiquetasData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                new { IdSolicitud = idSolicitud });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cr_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener(int codEmpresa, string usuario)
        {
            usuario = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe indicar el usuario.",
                    Result = []
                };
            }

            const string sqlQuery = @"
                SELECT
                    RTRIM(T.Tag_Codigo) AS item,
                    RTRIM(T.Descripcion) AS descripcion
                FROM CRD_TAGS T
                INNER JOIN CRD_TAGS_GRUPOS TG
                    ON TG.TAG_CODIGO = T.TAG_CODIGO
                INNER JOIN CRD_GRPUSERS GU
                    ON GU.COD_GRUPO = TG.COD_GRUPO
                WHERE T.ACTIVO = 1
                  AND GU.USUARIO = @Usuario;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                new { Usuario = usuario });
        }

        public ErrorDto Cr_SeguimientoEtiquetas_Aplicar(int codEmpresa, CrSeguimientoEtiquetasAplicarRequest request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("La solicitud es requerida.");
            }

            const string sql = "EXEC spCrdOperacionTagRegistra @Operacion, @CrdLinea, @Tag, @Usuario, @Asignado, @Notas;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    request.Operacion,
                    CrdLinea = request.Linea,
                    request.Tag,
                    request.Usuario,
                    request.Asignado,
                    request.Notas
                });
        }

        public ErrorDto<int> Cr_SeguimientoEtiquetas_NotaLargo_Obtener(int codEmpresa, string tagCodigo)
        {
            tagCodigo = (tagCodigo ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(tagCodigo))
            {
                return new ErrorDto<int>
                {
                    Code = -1,
                    Description = "Debe indicar la etiqueta.",
                    Result = 0
                };
            }

            const string sqlQuery = @"
                SELECT ISNULL(Nota_Largo, 0)
                FROM Crd_Tags
                WHERE Tag_Codigo = @TagCodigo;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                0,
                new { TagCodigo = tagCodigo });
        }
    }
}
