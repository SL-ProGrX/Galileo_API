using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPeriodosGraciaDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrPeriodosGraciaDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el catalogo de garantias.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Garantias_Obtener(int codEmpresa)
        {
            const string sqlGarantias = @"
                select
                    rtrim(Garantia) as item,
                    rtrim(descripcion) as descripcion
                from crd_garantia_tipos
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlGarantias
            );
        }

        /// <summary>
        /// Obtiene el catalogo de divisas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Divisas_Obtener(int codEmpresa)
        {
            const string sqlDivisas = @"
                select
                    rtrim(cod_divisa) as item,
                    rtrim(descripcion) as descripcion
                from vsys_divisas;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlDivisas
            );
        }

        /// <summary>
        /// Obtiene el catalogo de recursos (grupos).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="lineas"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Recursos_Obtener(
            int codEmpresa,
            bool lineas,
            string? codigo)
        {
            if (!lineas && string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe enviar el codigo cuando lineas es false.",
                    Result = []
                };
            }

            const string sqlRecursosLineas = @"
                select
                    rtrim(cod_grupo) as item,
                    rtrim(descripcion) as descripcion
                from catalogo_grupos
                order by descripcion;";

            const string sqlRecursosCodigo = @"
                select
                    rtrim(R.cod_grupo) as item,
                    rtrim(R.descripcion) as descripcion
                from catalogo_grupos R
                inner join catalogo_AsignaGrp A
                    on R.cod_grupo = A.cod_grupo
                where A.codigo = @Codigo
                order by R.descripcion;";

            if (lineas)
            {
                return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                    _portalDb,
                    codEmpresa,
                    sqlRecursosLineas
                );
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlRecursosCodigo,
                new
                {
                    Codigo = codigo!.Trim()
                }
            );
        }

        /// <summary>
        /// Obtiene el catalogo de destinos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="lineas"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Destinos_Obtener(
            int codEmpresa,
            bool lineas,
            string? codigo)
        {
            if (!lineas && string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe enviar el codigo cuando lineas es false.",
                    Result = []
                };
            }

            const string sqlDestinosLineas = @"
                select
                    rtrim(cod_destino) as item,
                    rtrim(descripcion) as descripcion
                from catalogo_destinos
                order by descripcion;";

            const string sqlDestinosCodigo = @"
                select
                    rtrim(R.cod_destino) as item,
                    rtrim(R.descripcion) as descripcion
                from catalogo_destinos R
                inner join catalogo_destinosAsg A
                    on R.cod_destino = A.cod_destino
                where A.codigo = @Codigo
                order by R.descripcion;";

            if (lineas)
            {
                return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                    _portalDb,
                    codEmpresa,
                    sqlDestinosLineas
                );
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlDestinosCodigo,
                new
                {
                    Codigo = codigo!.Trim()
                }
            );
        }

        /// <summary>
        /// Obtiene el catalogo de instituciones.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Instituciones_Obtener(int codEmpresa)
        {
            const string sqlInstituciones = @"
                select
                    rtrim(cod_institucion) as item,
                    rtrim(descripcion) as descripcion
                from instituciones
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlInstituciones
            );
        }

        /// <summary>
        /// Obtiene instituciones deductoras.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="todos"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Deductoras_Obtener(
            int codEmpresa,
            bool todos,
            string? codInstitucion)
        {
            if (todos)
                return CrPeriodosGracia_Instituciones_Obtener(codEmpresa);

            if (string.IsNullOrWhiteSpace(codInstitucion))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe enviar codInstitucion cuando todos es false.",
                    Result = []
                };
            }

            const string sqlDeductoras = @"
                exec spAFI_Institucion_Vinculadas
                    @CodInstitucion,
                    3;";

            var resp = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query<(string? IdX, string? ItmX)>(
                    sqlDeductoras,
                    new
                    {
                        CodInstitucion = codInstitucion.Trim()
                    }
                ).ToList()
            );

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = resp.Code,
                Description = resp.Description,
                Result =
                [
                    .. (resp.Result ?? [])
                        .Select(x => new DropDownListaGenericaModel
                        {
                            item = (x.IdX ?? string.Empty).Trim(),
                            descripcion = (x.ItmX ?? string.Empty).Trim()
                        })
                ]
            };
        }

        /// <summary>
        /// Obtiene el catalogo de estados de persona.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_EstadosPersona_Obtener(int codEmpresa)
        {
            const string sqlEstadosPersona = @"
                select
                    rtrim(cod_estado) as item,
                    rtrim(descripcion) as descripcion
                from afi_estados_persona
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlEstadosPersona
            );
        }

        /// <summary>
        /// Obtiene el catalogo de estado laboral activo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_EstadosLaborales_Obtener(int codEmpresa)
        {
            const string sqlEstadosLaborales = @"
                select
                    rtrim(Estado_Laboral) as item,
                    rtrim(Descripcion) as descripcion
                from AFI_ESTADO_LABORAL
                where Activo = 1
                order by Descripcion asc;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlEstadosLaborales
            );
        }

        /// <summary>
        /// Obtiene el catalogo de lineas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Lineas_Obtener(int codEmpresa)
        {
            const string sqlLineas = @"
                select
                    rtrim(codigo) as item,
                    rtrim(descripcion) as descripcion
                from catalogo
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlLineas
            );
        }
    }
}
