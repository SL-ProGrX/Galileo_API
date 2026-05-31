using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmAFInstitucionesDB
    {
        private const string SqlInstitucionesLista = @"
                    SELECT cod_institucion AS item,
                           descripcion
                    FROM dbo.instituciones;";

        private const string SqlInstitucionScrollSiguiente = @"
                    SELECT TOP 1 cod_institucion
                    FROM dbo.instituciones
                    WHERE cod_institucion > @CodInstitucion
                    ORDER BY cod_institucion ASC;";

        private const string SqlInstitucionScrollAnterior = @"
                    SELECT TOP 1 cod_institucion
                    FROM dbo.instituciones
                    WHERE cod_institucion < @CodInstitucion
                    ORDER BY cod_institucion DESC;";

        private const string SqlInstitucionObtener = @"
                    SELECT *
                    FROM dbo.vAFI_Instituciones
                    WHERE cod_institucion = @CodInstitucion;";

        private const string SqlTiposAsientos = @"
                    SELECT RTRIM(Tipo_Asiento) AS item,
                           RTRIM(Tipo_Asiento) AS descripcion
                    FROM dbo.CntX_Tipos_Asientos
                    WHERE cod_contabilidad = @Conta;";

        private const string SqlOperadoras = @"
                    SELECT COD_OPERADORA AS item,
                           RTRIM(DESCRIPCION) AS descripcion
                    FROM dbo.FND_OPERADORAS;";

        private const string SqlDivisas = @"
                    SELECT RTRIM(cod_Divisa) AS item,
                           RTRIM(Descripcion) AS descripcion
                    FROM dbo.vSys_Divisas;";

        private const string SqlPlanes = @"
                    SELECT cod_plan AS item,
                           Descripcion
                    FROM dbo.fnd_planes
                    WHERE (@CodOperadora = 0 OR cod_operadora = @CodOperadora)
                      AND (@CodMoneda = '' OR cod_Moneda = @CodMoneda)
                    ORDER BY COD_PLAN;";

        private const string SpInstitucionVinculadas = "spAFI_Institucion_Vinculadas";
        private const string SpInstitucionesCodigosLineas = "spAFI_Instituciones_Codigos_Lineas";
        private const string SpInstitucionDepartamentos = "spAFI_Institucion_Departamentos";
        private const string SpInstitucionSecciones = "spAFI_Institucion_Secciones";

        private const string SqlCodigosDeduccion = @"
                    SELECT COD_DEDUCCION,
                           descripcion,
                           activo,
                           COD_INSTITUCION
                    FROM dbo.AFI_INSTITUCIONES_CODIGOS
                    WHERE COD_INSTITUCION = @CodInstitucion
                    ORDER BY COD_DEDUCCION;";

        private static readonly IReadOnlyDictionary<string, string> ComboSqlMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["A"] = SqlTiposAsientos,
            ["O"] = SqlOperadoras,
            ["D"] = SqlDivisas
        };

        /// <summary>
        /// Obtener lista de instituciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Lista_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstitucionesLista);
        }

        /// <summary>
        /// Navegar al siguiente o anterior codigo de institución mediante el ScrollCode, según corresponda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ScrollCode"></param>
        /// <param name="CodInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<AfInstitucionDto?> AF_Instituciones_Scroll_Obtener(int CodEmpresa, int ScrollCode, int CodInstitucion)
        {
            var sql = ScrollCode == 1 ? SqlInstitucionScrollSiguiente : SqlInstitucionScrollAnterior;
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                sql,
                0,
                new { CodInstitucion });

            return result.Code == 0
                ? AF_Institucion_Obtener(CodEmpresa, result.Result)
                : DbHelper.CreateErrorResponse<AfInstitucionDto?>(
                    result.Description ?? "Error al navegar instituciones.",
                    result.Code.GetValueOrDefault(-1),
                    null!);
        }

        /// <summary>
        /// Obtener información de la institución mediante el código
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<AfInstitucionDto?> AF_Institucion_Obtener(int CodEmpresa, int CodInstitucion)
        {
            return DbHelper.ExecuteSingleQuery<AfInstitucionDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstitucionObtener,
                null,
                new { CodInstitucion });
        }

        /// <summary>
        /// Obtener lista de tipos de asientos, operadoras o divisas según corresponda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Tipo"></param>
        /// <param name="Conta"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_CargaCombo_Obtener(int CodEmpresa, string Tipo, int Conta)
        {
            var tipoSeguro = NormalizarTexto(Tipo).ToUpperInvariant();
            if (!ComboSqlMap.TryGetValue(tipoSeguro, out var sql))
            {
                return DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>());
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                sql,
                new { Conta });
        }

        /// <summary>
        /// Obtener lista de planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOperadora"></param>
        /// <param name="CodMoneda"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Planes_Obtener(int CodEmpresa, int CodOperadora, string CodMoneda)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPlanes,
                new
                {
                    CodOperadora,
                    CodMoneda = NormalizarTexto(CodMoneda)
                });
        }

        /// <summary>
        /// Obtener lista de empresas vinculadas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="Tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfInstitucionEmpresasDto>> AF_Institucion_Empresas_Obtener(int CodEmpresa, int CodInstitucion, int Tipo)
        {
            return EjecutarStoredProcedureList<AfInstitucionEmpresasDto>(
                CodEmpresa,
                SpInstitucionVinculadas,
                new { CodInstitucion, Tipo });
        }

        /// <summary>
        /// Obtener lista de codigos de deducción
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<AfInstitucionesCodigosDto>> AF_Instituciones_Codigos_Obtener(int CodEmpresa, int CodInstitucion)
        {
            return DbHelper.ExecuteListQuery<AfInstitucionesCodigosDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlCodigosDeduccion,
                new { CodInstitucion });
        }

        /// <summary>
        /// Obtener listas de lineas vinculadas al codigo de deducción
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="Codigo"></param>
        /// <param name="rbCodigo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfInstitucionesCodigosLineasDto>> AF_Instituciones_Codigos_Lineas_Obtener(int CodEmpresa, int CodInstitucion, string Codigo, int rbCodigo)
        {
            return EjecutarStoredProcedureList<AfInstitucionesCodigosLineasDto>(
                CodEmpresa,
                SpInstitucionesCodigosLineas,
                new
                {
                    CodInstitucion,
                    Codigo = NormalizarTexto(Codigo),
                    Estado = ObtenerEstadoCodigo(rbCodigo)
                });
        }

        /// <summary>
        /// Obtener lista de departamentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<AfInstitucionDepartamentosDto>> AF_Institucion_Departamentos_Obtener(int CodEmpresa, int CodInstitucion)
        {
            return EjecutarStoredProcedureList<AfInstitucionDepartamentosDto>(
                CodEmpresa,
                SpInstitucionDepartamentos,
                new { CodInstitucion });
        }

        /// <summary>
        /// Obtener lista de secciones asociadas a un departamento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="CodDepartamento"></param>
        /// <returns></returns>
        public ErrorDto<List<AfInstitucionSeccionesDto>> AF_Institucion_Secciones_Obtener(int CodEmpresa, int CodInstitucion, string CodDepartamento)
        {
            return EjecutarStoredProcedureList<AfInstitucionSeccionesDto>(
                CodEmpresa,
                SpInstitucionSecciones,
                new
                {
                    CodInstitucion,
                    CodDepartamento = NormalizarDepartamento(CodDepartamento)
                });
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado que retorna una lista.
        /// </summary>
        private ErrorDto<List<T>> EjecutarStoredProcedureList<T>(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<T>(storedProcedure, parameters, commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al ejecutar procedimiento almacenado.", result.Code.GetValueOrDefault(-1), new List<T>());
        }

        /// <summary>
        /// Normaliza el departamento recibido desde pantalla.
        /// </summary>
        private static string NormalizarDepartamento(string? departamento)
        {
            return string.Equals(NormalizarTexto(departamento), "N/A", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : NormalizarTexto(departamento);
        }

        /// <summary>
        /// Obtiene el estado de los códigos según el radio seleccionado.
        /// </summary>
        private static int? ObtenerEstadoCodigo(int rbCodigo)
        {
            return rbCodigo switch
            {
                1 => 1,
                2 => 0,
                _ => null
            };
        }
    }
}