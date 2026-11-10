using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;
using Galileo.Models.ProGrX.Clientes;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class FrmAfAjustesDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;
        private readonly MAfilicacionDB _mAfiliacion;

        private const string SpCatalogosConsulta = "spAF_Catalogos_Consulta";

        private const int ModuloBitacora = 9;
        private const int LongitudDetalleBitacora = 50;
        private const string MovimientoModifica = "MODIFICA";
        private const string ParametroBitacoraEspecial = "07";
        private const string MovimientoBitacoraIdentificacion = "26";
        private const string MovimientoBitacoraEstado = "27";
        private const string MovimientoBitacoraInstitucion = "01";

        private const string SqlDescripcionTipoIdSocio = @"
                    SELECT RTRIM(T.Descripcion)
                    FROM dbo.socios S
                    LEFT JOIN dbo.AFI_TIPOS_IDS T
                        ON S.tipo_id = T.tipo_id
                    WHERE S.cedula = @Cedula;";

        private const string SqlDescripcionTipoId = @"
                    SELECT RTRIM(Descripcion)
                    FROM dbo.AFI_TIPOS_IDS
                    WHERE tipo_id = @TipoId;";

        private const string SqlDescripcionEstadoSocio = @"
                    SELECT RTRIM(Est.Descripcion)
                    FROM dbo.socios S
                    INNER JOIN dbo.AFI_ESTADOS_PERSONA Est
                        ON S.EstadoActual = Est.Cod_Estado
                    WHERE S.cedula = @Cedula;";

        private const string SqlDescripcionEstado = @"
                    SELECT RTRIM(Descripcion)
                    FROM dbo.AFI_ESTADOS_PERSONA
                    WHERE Cod_Estado = @Estado;";

        private const string SqlUbicacionSocio = @"
                    SELECT cod_institucion AS CodInstitucion,
                           RTRIM(cod_departamento) AS CodDepartamento,
                           RTRIM(cod_seccion) AS CodSeccion
                    FROM dbo.socios
                    WHERE cedula = @Cedula;";

        private const string SqlParametroBitacoraEspecial = @"
                    SELECT TOP 1 RTRIM(valor)
                    FROM dbo.afi_parametros
                    WHERE cod_parametro = @Codigo;";

        private const string SqlInstituciones = @"
                    SELECT COD_INSTITUCION AS item,
                           RTRIM(Descripcion) AS descripcion
                    FROM dbo.INSTITUCIONES
                    ORDER BY COD_INSTITUCION;";

        private const string SqlTiposId = @"
                    SELECT TIPO_ID AS item,
                           RTRIM(Descripcion) AS descripcion
                    FROM dbo.AFI_TIPOS_IDS;";

        private const string SqlEstadosPersonaActivos = @"
                    SELECT RTRIM(E.COD_ESTADO) AS item,
                           RTRIM(E.DESCRIPCION) AS Descripcion
                    FROM dbo.AFI_ESTADOS_PERSONA E
                    WHERE E.ACTIVO = 1;";

        private const string SqlCambiarIdentificacion = @"
                    UPDATE dbo.socios
                    SET tipo_id = @TipoId
                    WHERE cedula = @Cedula;";

        private const string SqlEstadoAutorizadoSocio = @"
                    SELECT COUNT(1)
                    FROM dbo.AFI_ESTADOS_INSTITUCIONES
                    WHERE cod_estado = @Estado
                      AND cod_institucion IN
                      (
                          SELECT cod_institucion
                          FROM dbo.socios
                          WHERE cedula = @Cedula
                      );";

        private const string SqlAporteSocio = @"
                    SELECT TOP 1 ISNULL(aporte, 0)
                    FROM dbo.Ahorro_consolidado
                    WHERE cedula = @Cedula
                      AND Aporte > 0;";

        private const string SqlCambiarEstado = @"
                    UPDATE dbo.socios
                    SET estadoActual = @Estado
                    WHERE cedula = @Cedula;";

        private const string SqlEstadoPermitidoInstitucion = @"
                    SELECT COUNT(1)
                    FROM dbo.AFI_ESTADOS_INSTITUCIONES
                    WHERE cod_institucion = @CodInst
                      AND cod_estado IN
                      (
                          SELECT estadoActual
                          FROM dbo.socios
                          WHERE cedula = @Cedula
                      );";

        private const string SqlCambiarInstitucionAseccss = @"
                    UPDATE dbo.socios
                    SET cod_institucion = COALESCE(@CodInst, cod_institucion),
                        UP = CASE WHEN @CambiarUP = 1 THEN @UP ELSE UP END,
                        UT = CASE WHEN @CambiarUT = 1 THEN @UT ELSE UT END,
                        CT = CASE WHEN @CambiarCT = 1 THEN @CT ELSE CT END
                    WHERE cedula = @Cedula;";

        private const string SqlCambiarInstitucion = @"
                    UPDATE dbo.socios
                    SET cod_institucion = @CodInst,
                        cod_departamento = @CodDept,
                        cod_seccion = @CodSec
                    WHERE cedula = @Cedula;";

        private const string SqlCargarDatosAjuste = @"
                    SELECT S.*,
                           Est.Descripcion AS EstadoPersonaDesc,
                           Est.Cod_Estado + ' - ' + Est.Descripcion AS EstadoPersona,
                           I.descripcion AS DescInst,
                           D.descripcion AS DescDept,
                           X.descripcion AS DescSec,
                           Tid.Descripcion AS TipoIdDesc
                    FROM dbo.socios S
                    INNER JOIN dbo.Instituciones I
                        ON S.cod_institucion = I.cod_institucion
                    LEFT JOIN dbo.AFDepartamentos D
                        ON S.cod_institucion = D.cod_institucion
                       AND S.cod_departamento = D.cod_departamento
                    LEFT JOIN dbo.AFSecciones X
                        ON S.cod_institucion = X.cod_institucion
                       AND S.cod_departamento = X.cod_departamento
                       AND S.cod_seccion = X.cod_seccion
                    INNER JOIN dbo.AFI_ESTADOS_PERSONA Est
                        ON S.EstadoActual = Est.Cod_Estado
                    LEFT JOIN dbo.AFI_TIPOS_IDS Tid
                        ON S.tipo_id = Tid.tipo_id
                    WHERE S.cedula = @Cedula;";

        public FrmAfAjustesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
            _mAfiliacion = new MAfilicacionDB(_config);
        }


        /// <summary>
        /// Obtiene las instituciones disponibles para ajustes de clientes.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de instituciones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstituciones);
        }


        /// <summary>
        /// Obtiene los tipos de identificación disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de tipos de identificación.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_TiposId_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlTiposId);
        }


        /// <summary>
        /// Obtiene los estados de persona activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de estados activos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_EstadosPersona_ObtenerActivos(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlEstadosPersonaActivos);
        }

        /// <summary>
        /// Construye una respuesta estándar a partir de una ejecución WithConn que retorna ErrorDto.
        /// </summary>
        private static ErrorDto CrearRespuestaOperacion(ErrorDto<ErrorDto> result, string errorMessage)
        {
            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Actualiza el tipo de identificación de una persona.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="nuevoTipoId">Nuevo tipo de identificación.</param>
        /// <param name="usuario">Usuario que realiza el cambio (bitácora).</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto AF_Ajustes_CambiarIdentificacion(int CodEmpresa, string cedula, int nuevoTipoId, string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var cedulaSegura = NormalizarTexto(cedula);
                var tipoAnterior = connection.QueryFirstOrDefault<string>(SqlDescripcionTipoIdSocio, new { Cedula = cedulaSegura }) ?? string.Empty;
                var tipoNuevo = connection.QueryFirstOrDefault<string>(SqlDescripcionTipoId, new { TipoId = nuevoTipoId }) ?? string.Empty;

                connection.Execute(SqlCambiarIdentificacion, new
                {
                    TipoId = nuevoTipoId,
                    Cedula = cedulaSegura
                });

                var detalle = $"Identificación de {tipoAnterior} a {tipoNuevo} {cedulaSegura}";
                RegistrarBitacoraGeneral(CodEmpresa, usuario, detalle);
                RegistrarBitacoraEspecialSiHabilitada(connection, CodEmpresa, MovimientoBitacoraIdentificacion, detalle, cedulaSegura, usuario);

                return DbHelper.OkResponse("Actualizado correctamente");
            });

            return CrearRespuestaOperacion(result, "Error al actualizar identificación.");
        }


        /// <summary>
        /// Cambia el estado actual de una persona después de validar reglas de negocio.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="nuevoEstado">Nuevo estado solicitado.</param>
        /// <param name="usuario">Usuario que realiza el cambio (bitácora).</param>
        /// <returns>Resultado del cambio de estado.</returns>
        public ErrorDto AF_Ajustes_CambiarEstado(int CodEmpresa, string cedula, string nuevoEstado, string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var cedulaSegura = NormalizarTexto(cedula);
                var estadoSeguro = NormalizarTexto(nuevoEstado);

                if (!EstadoAutorizadoSocio(connection, cedulaSegura, estadoSeguro))
                {
                    return DbHelper.ErrorResponse("El estado indicado no está autorizado en la institución del socio.");
                }

                if (TieneAporteRegistrado(connection, cedulaSegura))
                {
                    return DbHelper.ErrorResponse("No procede el cambio de estado porque la persona tiene Aporte registrado.");
                }

                var estadoAnterior = connection.QueryFirstOrDefault<string>(SqlDescripcionEstadoSocio, new { Cedula = cedulaSegura }) ?? string.Empty;
                var estadoNuevo = connection.QueryFirstOrDefault<string>(SqlDescripcionEstado, new { Estado = estadoSeguro }) ?? string.Empty;

                connection.Execute(SqlCambiarEstado, new
                {
                    Estado = estadoSeguro,
                    Cedula = cedulaSegura
                });

                // Texto idéntico al VB6 (sbCambioEstado), incluido el doble espacio.
                var detalle = $"Estado de  de {estadoAnterior} a {estadoNuevo} {cedulaSegura}";
                RegistrarBitacoraGeneral(CodEmpresa, usuario, detalle);
                RegistrarBitacoraEspecialSiHabilitada(connection, CodEmpresa, MovimientoBitacoraEstado, detalle, cedulaSegura, usuario);

                return DbHelper.OkResponse("Estado actualizado correctamente");
            });

            return CrearRespuestaOperacion(result, "Error al cambiar estado.");       
        }


        /// <summary>
        /// Cambia institución y campos ASECCSS opcionales de una persona.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="codInstitucion">Nueva institución, si aplica.</param>
        /// <param name="up">Nueva UP, si aplica.</param>
        /// <param name="ut">Nueva UT, si aplica.</param>
        /// <param name="ct">Nueva CT, si aplica.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto AF_Ajustes_CambiarInstitucion_ASECCSS(
            int CodEmpresa,
            string cedula,
            int? codInstitucion,
            string? up,
            string? ut,
            string? ct
        )
        {
            if (!codInstitucion.HasValue && string.IsNullOrWhiteSpace(up) && string.IsNullOrWhiteSpace(ut) && string.IsNullOrWhiteSpace(ct))
            {
                return DbHelper.OkResponse("No hay cambios para aplicar.");
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var cedulaSegura = NormalizarTexto(cedula);

                if (codInstitucion.HasValue && !EstadoPermitidoInstitucion(connection, cedulaSegura, codInstitucion.Value))
                {
                    return DbHelper.ErrorResponse("El estado actual del socio no está autorizado en la institución indicada.");
                }

                connection.Execute(SqlCambiarInstitucionAseccss, new
                {
                    CodInst = codInstitucion,
                    CambiarUP = !string.IsNullOrWhiteSpace(up) ? 1 : 0,
                    UP = NormalizarTexto(up),
                    CambiarUT = !string.IsNullOrWhiteSpace(ut) ? 1 : 0,
                    UT = NormalizarTexto(ut),
                    CambiarCT = !string.IsNullOrWhiteSpace(ct) ? 1 : 0,
                    CT = NormalizarTexto(ct),
                    Cedula = cedulaSegura
                });

                return DbHelper.OkResponse("Actualización aplicada correctamente.");
            });

            return CrearRespuestaOperacion(result, "Error al cambiar institución.");
        }


        /// <summary>
        /// Cambia institución, departamento y sección de una persona.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="codInstitucion">Nueva institución.</param>
        /// <param name="codDepartamento">Nuevo departamento.</param>
        /// <param name="codSeccion">Nueva sección.</param>
        /// <param name="usuario">Usuario que realiza el cambio (bitácora).</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto AF_Ajustes_CambiarInstitucion(
            int CodEmpresa,
            string cedula,
            int codInstitucion,
            string codDepartamento,
            string codSeccion,
            string usuario
        )
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var cedulaSegura = NormalizarTexto(cedula);

                if (!EstadoPermitidoInstitucion(connection, cedulaSegura, codInstitucion))
                {
                    return DbHelper.ErrorResponse("El estado actual del socio no está autorizado en la institución indicada.");
                }

                var deptNuevo = NormalizarTexto(codDepartamento);
                var seccionNueva = NormalizarTexto(codSeccion);
                var ubicacionAnterior = connection.QueryFirstOrDefault<UbicacionSocio>(SqlUbicacionSocio, new { Cedula = cedulaSegura });

                connection.Execute(SqlCambiarInstitucion, new
                {
                    CodInst = codInstitucion,
                    CodDept = deptNuevo,
                    CodSec = seccionNueva,
                    Cedula = cedulaSegura
                });

                // VB6 sbCambioinstitucion: bitácora especial "01" solo si hubo cambios; sin bitácora general.
                var detalle = ConstruirDetalleInstitucion(ubicacionAnterior, codInstitucion, deptNuevo, seccionNueva);
                if (detalle.Length > 0)
                {
                    _mAfiliacion.sbgAFIBitacora_Registrar(CodEmpresa, MovimientoBitacoraInstitucion, detalle, cedulaSegura, usuario);
                }

                return DbHelper.OkResponse("Institución y dependencias actualizadas correctamente.");
            });

            return CrearRespuestaOperacion(result, "Error al cambiar institución.");
        }


        /// <summary>
        /// Carga el detalle de una persona para ajustes.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Detalle de la persona para ajustes.</returns>
        public ErrorDto<AfAjustePersonaDetalle> AF_Ajustes_CargarDatos(int CodEmpresa, string cedula)
        {
            var result = DbHelper.ExecuteSingleQuery<AfAjustePersonaDetalle>(
                CreatePortalDb(),
                CodEmpresa,
                SqlCargarDatosAjuste,
                null,
                new { Cedula = NormalizarTexto(cedula) });

            if (result.Code != 0)
            {
                return new ErrorDto<AfAjustePersonaDetalle>
                {
                    Code = result.Code,
                    Description = result.Description,
                    Result = result.Result
                };
            }

            if (result.Result is null)
            {
                return DbHelper.CreateErrorResponse<AfAjustePersonaDetalle>("No se encontraron datos para la cédula indicada.", 1, null);
            }

            return new ErrorDto<AfAjustePersonaDetalle>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result
            };
        }


        /// <summary>
        /// Aplica ajustes de identificación, estado o institución según el código solicitado.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="ajuste">JSON con los datos del ajuste.</param>
        /// <param name="codigo">Código del tipo de ajuste.</param>
        /// <returns>Resultado del ajuste.</returns>
        public ErrorDto AF_Ajustes_Cambiar(int CodEmpresa, string ajuste, int codigo)
        {
            var request = DbHelper.DeserializeOrNew<AFAjuste>(ajuste);
            return codigo switch
            {
                1 => AF_Ajustes_CambiarIdentificacion(CodEmpresa, request.cedula, request.nuevo_tipo_id, request.usuario),
                2 => AF_Ajustes_CambiarEstado(CodEmpresa, request.cedula, request.nuevo_estado, request.usuario),
                3 => AF_Ajustes_CambiarInstitucion(CodEmpresa, request.cedula, request.cod_institucion, request.cod_departamento, request.cod_seccion, request.usuario),
                _ => DbHelper.ErrorResponse("Código de ajuste no válido.")
            };
        }


        /// <summary>
        /// Obtiene los catálogos generales requeridos para ajustes de clientes.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_institucion">Código de institución para filtrar dependencias.</param>
        /// <returns>Catálogos generales para ajustes.</returns>
        public ErrorDto<AfCatalogosGeneralesDto> AF_Catalogos_Obtener(int CodEmpresa, string? cod_institucion)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                using var multi = connection.QueryMultiple(
                    SpCatalogosConsulta,
                    new { cod_institucion = NormalizarInstitucion(cod_institucion) },
                    commandType: CommandType.StoredProcedure);

                return LeerCatalogosGenerales(multi);
            });

            return new ErrorDto<AfCatalogosGeneralesDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new AfCatalogosGeneralesDto()
            };
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Ubicación de la persona (institución, departamento y sección) antes de un cambio.
        /// </summary>
        public sealed class UbicacionSocio
        {
            public int CodInstitucion { get; set; }
            public string? CodDepartamento { get; set; }
            public string? CodSeccion { get; set; }
        }

        /// <summary>
        /// Registra en la bitácora general (equivale al Bitacora de VB6): movimiento MODIFICA y detalle en mayúsculas, máximo 50 caracteres.
        /// </summary>
        private void RegistrarBitacoraGeneral(int codEmpresa, string usuario, string detalle)
        {
            var detalleBitacora = detalle.ToUpperInvariant();
            if (detalleBitacora.Length > LongitudDetalleBitacora)
            {
                detalleBitacora = detalleBitacora[..LongitudDetalleBitacora];
            }

            _mSecurity.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = detalleBitacora,
                Movimiento = MovimientoModifica,
                Modulo = ModuloBitacora
            });
        }

        /// <summary>
        /// Registra en la bitácora especial solo si el parámetro 07 (BitacoraEspecial de VB6) está en "S".
        /// </summary>
        private void RegistrarBitacoraEspecialSiHabilitada(SqlConnection connection, int codEmpresa, string movimiento, string detalle, string cedula, string usuario)
        {
            var habilitada = connection.QueryFirstOrDefault<string>(SqlParametroBitacoraEspecial, new { Codigo = ParametroBitacoraEspecial }) == "S";
            if (habilitada)
            {
                _mAfiliacion.sbgAFIBitacora_Registrar(codEmpresa, movimiento, detalle, cedula, usuario);
            }
        }

        /// <summary>
        /// Construye el detalle de cambios de institución con el formato de VB6; vacío si no hubo cambios.
        /// </summary>
        private static string ConstruirDetalleInstitucion(UbicacionSocio? anterior, int codInstitucion, string codDepartamento, string codSeccion)
        {
            var actual = anterior ?? new UbicacionSocio();
            var departamentoAnterior = actual.CodDepartamento?.Trim() ?? string.Empty;
            var seccionAnterior = actual.CodSeccion?.Trim() ?? string.Empty;
            var cambios = new List<string>();

            if (actual.CodInstitucion != codInstitucion)
            {
                cambios.Add($"Institución: {actual.CodInstitucion} -> {codInstitucion}");
            }
            if (departamentoAnterior != codDepartamento)
            {
                cambios.Add($"¦ Dept: {departamentoAnterior} -> {codDepartamento}");
            }
            if (seccionAnterior != codSeccion)
            {
                cambios.Add($"¦ Sección: {seccionAnterior} -> {codSeccion}");
            }

            return string.Concat(cambios);
        }

        /// <summary>
        /// Valida si el estado solicitado está autorizado para la institución actual del socio.
        /// </summary>
        private static bool EstadoAutorizadoSocio(SqlConnection connection, string cedula, string estado)
        {
            return connection.QueryFirstOrDefault<int>(SqlEstadoAutorizadoSocio, new
            {
                Estado = estado,
                Cedula = cedula
            }) > 0;
        }

        /// <summary>
        /// Valida si la persona tiene aportes registrados.
        /// </summary>
        private static bool TieneAporteRegistrado(SqlConnection connection, string cedula)
        {
            return connection.QueryFirstOrDefault<decimal>(SqlAporteSocio, new { Cedula = cedula }) > 0;
        }

        /// <summary>
        /// Valida si el estado actual del socio está permitido en la nueva institución.
        /// </summary>
        private static bool EstadoPermitidoInstitucion(SqlConnection connection, string cedula, int codInstitucion)
        {
            return connection.QueryFirstOrDefault<int>(SqlEstadoPermitidoInstitucion, new
            {
                CodInst = codInstitucion,
                Cedula = cedula
            }) > 0;
        }

        /// <summary>
        /// Normaliza el código de institución recibido desde la interfaz.
        /// </summary>
        private static string? NormalizarInstitucion(string? codInstitucion)
        {
            return string.IsNullOrWhiteSpace(codInstitucion) || codInstitucion == "undefined"
                ? null
                : codInstitucion.Trim();
        }

        /// <summary>
        /// Lee todos los result sets devueltos por el procedimiento de catálogos.
        /// </summary>
        private static AfCatalogosGeneralesDto LeerCatalogosGenerales(SqlMapper.GridReader multi)
        {
            return new AfCatalogosGeneralesDto
            {
                EstadoCivil = multi.Read<DropDownListaGenericaModel>().ToList(),
                Divisas = multi.Read<DropDownListaGenericaModel>().ToList(),
                TiposIdentificacion = multi.Read<DropDownListaGenericaModel>().ToList(),
                Profesiones = multi.Read<DropDownListaGenericaModel>().ToList(),
                Sectores = multi.Read<DropDownListaGenericaModel>().ToList(),
                Sociedades = multi.Read<DropDownListaGenericaModel>().ToList(),
                ActividadesEconomicas = multi.Read<DropDownListaGenericaModel>().ToList(),
                Paises = multi.Read<DropDownListaGenericaModel>().ToList(),
                EstadosPersonaIngreso = multi.Read<DropDownListaGenericaModel>().ToList(),
                Nacionalidades = multi.Read<DropDownListaGenericaModel>().ToList(),
                NivelAcademico = multi.Read<DropDownListaGenericaModel>().ToList(),
                EstadoLaboral = multi.Read<DropDownListaGenericaModel>().ToList(),
                ActividadLaboral = multi.Read<DropDownListaGenericaModel>().ToList(),
                RelacionParentesco = multi.Read<DropDownListaGenericaModel>().ToList(),
                Promotores = multi.Read<DropDownListaGenericaModel>().ToList(),
                Instituciones = multi.Read<DropDownListaGenericaModel>().ToList(),
                Deductoras = multi.Read<DropDownListaGenericaModel>().ToList(),
                Departamentos = multi.Read<DropDownListaGenericaModel>().ToList(),
                Secciones = multi.Read<DropDownListaGenericaModel>().ToList()
            };
        }

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

    }
}