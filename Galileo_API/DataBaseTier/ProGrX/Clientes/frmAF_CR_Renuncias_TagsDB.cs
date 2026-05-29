using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfCrRenunciasTagsDB
    {
        private readonly IConfiguration _config;

        private const string SpRenunciasControlConsulta = "spAFI_Renuncias_Control_Consulta";
        private const string SpRenunciaRecepcionAplica = "spAFI_Renuncia_Recepcion_Aplica";
        private const string SpRenunciaRevisionAplica = "spAFI_Renuncia_Revision_Aplica";
        private const string SpRenunciaEtiquetasConsulta = "spAFI_Renuncia_Etiquetas_Consulta";
        private const string SpRenunciaRevisionReversar = "spAFI_Renuncia_Revision_Reversar";

        private const string SqlRenunciaRevisionReversarValida = @"
                    SELECT dbo.fxAFI_Renuncia_Revision_Reversar_Valida(@RenunciaId);";

        private const string SqlRenunciasPendientes = @"
                    SELECT COD_RENUNCIA AS Cod_Renuncia,
                           CEDULA,
                           NOMBRE,
                           Estado_Desc
                    FROM dbo.vAFI_Renuncias_Pendientes_Recibir;";

        private const string SqlRenuncias = @"
                    SELECT COD_RENUNCIA AS Cod_Renuncia,
                           CEDULA,
                           NOMBRE,
                           Estado_Desc
                    FROM dbo.vAFI_Renuncias;";

        public FrmAfCrRenunciasTagsDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista de renuncias según estado y filtro aplicado.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Estado">Estado de las renuncias.</param>
        /// <param name="Filtro">Texto de búsqueda.</param>
        /// <returns>Listado de renuncias filtradas.</returns>
        public ErrorDto<List<AfCrRenunciasTagsData>> AF_CR_Renuncias_Tags_Obtener(int CodEmpresa, string Estado, string Filtro)
        {
            return EjecutarStoredProcedureList<AfCrRenunciasTagsData>(
                CodEmpresa,
                SpRenunciasControlConsulta,
                new
                {
                    Estado = NormalizarTexto(Estado),
                    Filtro = NormalizarTexto(Filtro)
                });
        }


        /// <summary>
        /// Aplica la recepción de una renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="recepcionDatos">Datos de recepción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_CR_Renuncia_Recepcion_Aplica(int CodEmpresa, AfCrRenunciaRecepcionAplica recepcionDatos)
        {
            if (recepcionDatos is null)
            {
                return DbHelper.ErrorResponse("Los datos de recepción son requeridos.", -2);
            }

            return EjecutarStoredProcedure(
                CodEmpresa,
                SpRenunciaRecepcionAplica,
                new
                {
                    recepcionDatos.RenunciaId,
                    Usuario = NormalizarTexto(recepcionDatos.Usuario),
                    Notas = NormalizarTexto(recepcionDatos.Notas),
                    Equipo = NormalizarTexto(recepcionDatos.Equipo),
                    Version = NormalizarTexto(recepcionDatos.Version)
                },
                "Error al aplicar recepción de renuncia.");
        }


        /// <summary>
        /// Aplica la revisión de una renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="revisionDatos">Datos de revisión.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_CR_Renuncia_Revision_Aplica(int CodEmpresa, AfCrRenunciaRevisionAplica revisionDatos)
        {
            if (revisionDatos is null)
            {
                return DbHelper.ErrorResponse("Los datos de revisión son requeridos.", -2);
            }

            return EjecutarStoredProcedure(
                CodEmpresa,
                SpRenunciaRevisionAplica,
                new
                {
                    revisionDatos.RenunciaId,
                    Usuario = NormalizarTexto(revisionDatos.Usuario),
                    Notas = NormalizarTexto(revisionDatos.Notas),
                    Equipo = NormalizarTexto(revisionDatos.Equipo),
                    Version = NormalizarTexto(revisionDatos.Version),
                    Estado = NormalizarTexto(revisionDatos.Estado)
                },
                "Error al aplicar revisión de renuncia.");
        }


        /// <summary>
        /// Obtiene las etiquetas asociadas a una renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="RenunciaId">Identificador de renuncia.</param>
        /// <returns>Listado de etiquetas asociadas.</returns>
        public ErrorDto<List<AfCrRenunciaEtiquetas>> AF_CR_Renuncia_Etiquetas_Consulta(int CodEmpresa, int RenunciaId)
        {
            return EjecutarStoredProcedureList<AfCrRenunciaEtiquetas>(
                CodEmpresa,
                SpRenunciaEtiquetasConsulta,
                new { RenunciaId });
        }


        /// <summary>
        /// Valida si una revisión de renuncia puede ser reversada.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="RenunciaId">Identificador de renuncia.</param>
        /// <returns>Resultado de validación.</returns>
        public ErrorDto<int> AF_CR_Renuncia_Revision_Reversar_Valida(int CodEmpresa, int RenunciaId)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRenunciaRevisionReversarValida,
                0,
                new { RenunciaId });

            return result.Code == 0
                ? result
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al validar reversa de revisión.",
                    result.Code.GetValueOrDefault(-1),
                    0);
        }


        /// <summary>
        /// Reversa la revisión de una renuncia.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="dto">Datos de reversa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_CR_Renuncia_Revision_Reversar(int CodEmpresa, AfCrRenunciaReversa dto)
        {
            if (dto is null)
            {
                return DbHelper.ErrorResponse("Los datos de reversa son requeridos.", -2);
            }

            return EjecutarStoredProcedure(
                CodEmpresa,
                SpRenunciaRevisionReversar,
                new
                {
                    dto.RenunciaId,
                    Usuario = NormalizarTexto(dto.Usuario),
                    NotasReversa = NormalizarTexto(dto.NotasReversa),
                    Equipo = NormalizarTexto(dto.Equipo),
                    Version = NormalizarTexto(dto.Version)
                },
                "Error al reversar revisión de renuncia.");
        }


        /// <summary>
        /// Obtiene las renuncias pendientes de recepción.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de renuncias pendientes.</returns>
        public ErrorDto<List<AfCrRenunciasTagsData>> AF_CR_Renuncias_Pendientes_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AfCrRenunciasTagsData>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRenunciasPendientes);
        }


        /// <summary>
        /// Obtiene todas las renuncias registradas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado completo de renuncias.</returns>
        public ErrorDto<List<AfCrRenunciasTagsData>> AF_CR_Renuncias_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AfCrRenunciasTagsData>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRenuncias);
        }


        /// <summary>
        /// Ejecuta un procedimiento almacenado sin retorno.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="storedProcedure">Nombre del procedimiento almacenado.</param>
        /// <param name="parameters">Parámetros del procedimiento.</param>
        /// <param name="errorMessage">Mensaje de error controlado.</param>
        /// <returns>Resultado de la ejecución.</returns>
        private ErrorDto EjecutarStoredProcedure(
            int codEmpresa,
            string storedProcedure,
            object parameters,
            string errorMessage)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Execute(
                    storedProcedure,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure);

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(errorMessage, result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Ejecuta un procedimiento almacenado que retorna una lista.
        /// </summary>
        /// <typeparam name="T">Tipo de resultado.</typeparam>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="storedProcedure">Nombre del procedimiento almacenado.</param>
        /// <param name="parameters">Parámetros del procedimiento.</param>
        /// <returns>Listado de resultados.</returns>
        private ErrorDto<List<T>> EjecutarStoredProcedureList<T>(
            int codEmpresa,
            string storedProcedure,
            object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<T>(
                    storedProcedure,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al ejecutar procedimiento almacenado.",
                    result.Code.GetValueOrDefault(-1),
                    new List<T>());
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}