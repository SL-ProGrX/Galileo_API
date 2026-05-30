using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using System.Data;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFPromotoresPrincipalDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;

        private const string SqlPromotoresLista = @"
                    SELECT id_promotor AS item,
                           nombre AS descripcion
                    FROM dbo.promotores;";

        private const string SqlUsuariosActivos = @"
                    SELECT NOMBRE AS item,
                           DESCRIPCION AS descripcion
                    FROM dbo.USUARIOS
                    WHERE ESTADO = 'A'
                    ORDER BY NOMBRE;";

        private const string SqlScrollSiguiente = @"
                    SELECT TOP 1 id_promotor
                    FROM dbo.promotores
                    WHERE id_promotor > @Codigo
                    ORDER BY id_promotor ASC;";

        private const string SqlScrollAnterior = @"
                    SELECT TOP 1 id_promotor
                    FROM dbo.promotores
                    WHERE id_promotor < @Codigo
                    ORDER BY id_promotor DESC;";

        private const string SqlPromotorObtener = @"
                    SELECT P.*,
                           B.descripcion AS Banco
                    FROM dbo.promotores P
                    INNER JOIN dbo.Tes_Bancos B
                        ON P.cod_banco = B.id_banco
                    WHERE P.id_promotor = @Codigo;";

        private const string SqlPromotorCuentas = @"
                    SELECT RTRIM(B.Descripcion) AS Banco,
                           CASE WHEN C.tipo = 'A' THEN 'Ahorros' ELSE 'Corriente' END AS TipoDesc,
                           C.cod_Divisa,
                           C.CUENTA_INTERNA,
                           C.CUENTA_INTERBANCA,
                           C.ACTIVA,
                           C.DESTINO,
                           C.REGISTRO_FECHA,
                           C.REGISTRO_USUARIO
                    FROM dbo.SYS_CUENTAS_BANCARIAS C
                    INNER JOIN dbo.TES_BANCOS_GRUPOS B
                        ON C.cod_banco = B.cod_grupo
                    WHERE C.Identificacion = @CodComision
                      AND C.Modulo = 'AFI';";

        private const string SpBancos = "spCrd_SGT_Bancos";

        private const string SqlPromotoresTotal = @"
                    SELECT COUNT(P.ID_PROMOTOR)
                    FROM dbo.Promotores P
                    INNER JOIN dbo.Tes_Bancos B
                        ON P.cod_banco = B.id_Banco
                    WHERE P.estado = @Estado
                      AND P.Tipo = @Tipo
                      AND (@hasFilter = 0 OR P.Nombre LIKE @filtro);";

        private const string SqlPromotoresListado = @"
                    SELECT P.*,
                           B.descripcion AS Banco
                    FROM dbo.Promotores P
                    INNER JOIN dbo.Tes_Bancos B
                        ON P.cod_banco = B.id_Banco
                    WHERE P.estado = @Estado
                      AND P.Tipo = @Tipo
                      AND (@hasFilter = 0 OR P.Nombre LIKE @filtro)
                    ORDER BY P.nombre ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlPromotorExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.promotores
                    WHERE id_promotor = @Codigo;";

        private const string SqlPromotorUpdate = @"
                    UPDATE dbo.promotores
                    SET nombre = @Nombre,
                        cedula_contacto = @CedJur,
                        nombre_contacto = @PagarA,
                        observacion = @Observacion,
                        estado = @Estado,
                        tipo_documento = @TipoDocumento,
                        direccion = @Direccion,
                        aptoPostal = @ApartadoPostal,
                        email = @Email,
                        telefono = @Telefono1,
                        telefono_ext = @TelefonoExt,
                        fax = @Fax,
                        fax_ext = @FaxExt,
                        cod_banco = @Banco,
                        comite = @Comite,
                        apl_comision = @Comision,
                        cod_comision = @CedJur,
                        Tipo = @Tipo,
                        user_referencia = @UsuarioRef,
                        usuario = @Usuario,
                        fecha = GETDATE()
                    WHERE id_promotor = @Codigo;";

        private const string SqlPromotorInsert = @"
                    INSERT INTO dbo.promotores
                    (
                        Tipo,
                        nombre,
                        observacion,
                        cod_comision,
                        fechaIng,
                        estado,
                        telefono,
                        telefono_ext,
                        fax,
                        fax_ext,
                        email,
                        aptopostal,
                        direccion,
                        tipo_documento,
                        cod_banco,
                        cedula_contacto,
                        nombre_contacto,
                        comite,
                        apl_comision,
                        usuario,
                        fecha,
                        user_referencia
                    )
                    VALUES
                    (
                        @Tipo,
                        @Nombre,
                        @Observacion,
                        @CedJur,
                        GETDATE(),
                        @Estado,
                        @Telefono1,
                        @TelefonoExt,
                        @Fax,
                        @FaxExt,
                        @Email,
                        @ApartadoPostal,
                        @Direccion,
                        @TipoDocumento,
                        @Banco,
                        @CedJur,
                        @PagarA,
                        @Comite,
                        @Comision,
                        @Usuario,
                        GETDATE(),
                        @UsuarioRef
                    );
                    SELECT ISNULL(MAX(id_promotor), 0) AS Ultimo
                    FROM dbo.promotores;";

        private const string SqlPromotorDelete = @"
                    DELETE FROM dbo.promotores
                    WHERE id_promotor = @Codigo;";

        public FrmAFPromotoresPrincipalDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la lista de promotores.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de promotores.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Promotores_Lista_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPromotoresLista);
        }


        /// <summary>
        /// Obtiene la lista de usuarios activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de usuarios activos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Promotores_Usuarios_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlUsuariosActivos);
        }


        /// <summary>
        /// Obtiene el promotor anterior o siguiente según el código de navegación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="ScrollCode">Código de navegación.</param>
        /// <param name="Codigo">Código base de promotor.</param>
        /// <returns>Datos del promotor encontrado.</returns>
        public ErrorDto<AfPromotoresPrincipalDto?> AF_Promotores_Scroll_Obtener(int CodEmpresa, int ScrollCode, int Codigo)
        {
            var sql = ScrollCode == 1 ? SqlScrollSiguiente : SqlScrollAnterior;
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                sql,
                0,
                new { Codigo });

            return result.Code == 0
                ? AF_Promotor_Obtener(CodEmpresa, result.Result)
                : DbHelper.CreateErrorResponse<AfPromotoresPrincipalDto?>(
                    result.Description ?? "Error al navegar promotores.",
                    result.Code.GetValueOrDefault(-1),
                    null);
        }


        /// <summary>
        /// Obtiene la información de un promotor por identificador.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Codigo">Identificador del promotor.</param>
        /// <returns>Datos del promotor.</returns>
        public ErrorDto<AfPromotoresPrincipalDto?> AF_Promotor_Obtener(int CodEmpresa, int Codigo)
        {
            return DbHelper.ExecuteSingleQuery<AfPromotoresPrincipalDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPromotorObtener,
                null,
                new { Codigo });
        }


        /// <summary>
        /// Obtiene las cuentas bancarias asociadas al promotor.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodComision">Código de comisión del promotor.</param>
        /// <returns>Listado de cuentas bancarias.</returns>
        public ErrorDto<List<AfPromotoresCuentasDto>> AF_Promotores_Cuentas_Obtener(int CodEmpresa, string CodComision)
        {
            return DbHelper.ExecuteListQuery<AfPromotoresCuentasDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPromotorCuentas,
                new { CodComision = NormalizarTexto(CodComision) });
        }


        /// <summary>
        /// Obtiene los bancos disponibles para el usuario.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que consulta.</param>
        /// <returns>Listado de bancos.</returns>
        public ErrorDto<List<AfPromotoresBancoDto>> AF_Promotores_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfPromotoresBancoDto>(
                    SpBancos,
                    new { usuario = NormalizarTexto(Usuario) },
                    commandType: CommandType.StoredProcedure).ToList());
        }


        /// <summary>
        /// Obtiene el listado paginado de promotores según tipo, estado y filtro.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Tipo">Tipo de promotor.</param>
        /// <param name="Estado">Estado del promotor.</param>
        /// <param name="filtros">Filtros de búsqueda y paginación.</param>
        /// <returns>Listado paginado de promotores.</returns>
        public ErrorDto<AfPromotoresPrincipalLista> AF_Promotores_ListadoConsulta_Obtener(int CodEmpresa, string Tipo, int Estado, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();
            var parameters = CrearParametrosListado(Tipo, Estado, filtros);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new AfPromotoresPrincipalLista
            {
                total = connection.QueryFirstOrDefault<int>(SqlPromotoresTotal, parameters),
                lista = connection.Query<AfPromotoresPrincipalDto>(SqlPromotoresListado, parameters).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener listado de promotores.",
                    result.Code.GetValueOrDefault(-1),
                    CrearListaVacia());
        }


        /// <summary>
        /// Inserta o actualiza un promotor.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="Info">Datos del promotor.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto AF_Promotores_Guardar(int CodEmpresa, string Usuario, AfPromotoresPrincipalDto Info)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos del promotor son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                GuardarPromotor(connection, CodEmpresa, Usuario, Info));

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar promotor.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Elimina un promotor.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="Codigo">Identificador del promotor.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto AF_Promotores_Eliminar(int CodEmpresa, string Usuario, int Codigo)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlPromotorDelete,
                new { Codigo });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar promotor.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraPromotor(CodEmpresa, Usuario, Codigo, "Elimina - WEB");
            return DbHelper.OkResponse("Ok");
        }


        /// <summary>
        /// Restringe la longitud máxima de un texto.
        /// </summary>
        /// <param name="value">Valor original.</param>
        /// <param name="maxLength">Longitud máxima permitida.</param>
        /// <returns>Texto truncado si excede la longitud máxima.</returns>
        private static string Truncate(string? value, int maxLength)
        {
            var texto = NormalizarTexto(value);
            return texto.Length <= maxLength ? texto : texto[..maxLength];
        }


        /// <summary>
        /// Guarda un promotor usando una conexión abierta.
        /// </summary>
        private ErrorDto GuardarPromotor(SqlConnection connection, int codEmpresa, string usuario, AfPromotoresPrincipalDto info)
        {
            var parametros = CrearParametrosPromotor(info, usuario);
            var existe = connection.QueryFirstOrDefault<int>(SqlPromotorExiste, new { Codigo = info.id_promotor });

            if (existe == 1)
            {
                connection.Execute(SqlPromotorUpdate, parametros);
                RegistrarBitacoraPromotor(codEmpresa, usuario, info.id_promotor, "Modifica - WEB");
                return DbHelper.OkResponse("Ok");
            }

            var ultimo = connection.QueryFirstOrDefault<int>(SqlPromotorInsert, parametros);
            RegistrarBitacoraPromotor(codEmpresa, usuario, ultimo, "Registra - WEB");
            return new ErrorDto { Code = ultimo, Description = "Ok" };
        }


        /// <summary>
        /// Crea parámetros seguros para guardar promotores.
        /// </summary>
        private static object CrearParametrosPromotor(AfPromotoresPrincipalDto info, string usuario)
        {
            return new
            {
                Nombre = Truncate(info.nombre, 60),
                CedJur = Truncate(info.cod_comision, 15),
                PagarA = Truncate(info.nombre_contacto, 60),
                Observacion = Truncate(info.observacion, 255),
                Estado = info.estado,
                TipoDocumento = Truncate(info.tipo_documento, 2),
                Direccion = Truncate(info.direccion, 255),
                ApartadoPostal = Truncate(info.aptopostal, 25),
                Email = Truncate(info.email, 100),
                Telefono1 = Truncate(info.telefono, 10),
                TelefonoExt = Truncate(info.telefono_ext, 5),
                Fax = Truncate(info.fax, 10),
                FaxExt = Truncate(info.fax_ext, 5),
                Banco = info.cod_banco,
                Comite = EsComite(info.tipo),
                Comision = info.apl_comision ? 1 : 0,
                Tipo = Truncate(info.tipo, 1),
                UsuarioRef = Truncate(info.user_referencia, 30),
                Usuario = Truncate(usuario, 30),
                Codigo = info.id_promotor
            };
        }


        /// <summary>
        /// Crea parámetros seguros para el listado de promotores.
        /// </summary>
        private static object CrearParametrosListado(string tipo, int estado, FiltrosLazyLoadData filtros)
        {
            var pageSize = Math.Max(1, filtros.paginacion);
            var filtroTexto = NormalizarTexto(filtros.filtro);

            return new
            {
                Tipo = NormalizarTexto(tipo),
                Estado = estado,
                hasFilter = string.IsNullOrWhiteSpace(filtroTexto) ? 0 : 1,
                filtro = string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%",
                offset = Math.Max(0, filtros.pagina),
                fetch = pageSize
            };
        }


        /// <summary>
        /// Crea una lista vacía de promotores.
        /// </summary>
        private static AfPromotoresPrincipalLista CrearListaVacia()
        {
            return new AfPromotoresPrincipalLista
            {
                total = 0,
                lista = new List<AfPromotoresPrincipalDto>()
            };
        }


        /// <summary>
        /// Registra en bitácora el movimiento de un promotor.
        /// </summary>
        private void RegistrarBitacoraPromotor(int codEmpresa, string usuario, int codigo, string movimiento)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = $"Ejecutivo de Cuenta Id: {codigo}",
                Movimiento = movimiento,
                Modulo = 9
            });
        }


        /// <summary>
        /// Indica si el tipo corresponde a comité.
        /// </summary>
        private static int EsComite(string? tipo)
        {
            return string.Equals(NormalizarTexto(tipo), "C", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}