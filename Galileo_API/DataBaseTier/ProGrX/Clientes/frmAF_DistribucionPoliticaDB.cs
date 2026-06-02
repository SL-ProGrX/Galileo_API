using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFDistribucionPoliticaDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;

        private const string SqlMascaraCanton = "SELECT MAX(LEN(canton)) AS Caracteres FROM dbo.CANTONES;";
        private const string SqlMascaraDistrito = "SELECT MAX(LEN(distrito)) AS Caracteres FROM dbo.Distritos;";

        private const string SqlProvincias = @"
                    SELECT provincia AS item,
                           descripcion
                    FROM dbo.provincias;";

        private const string SqlCantones = @"
                    SELECT canton AS item,
                           descripcion
                    FROM dbo.cantones
                    WHERE Provincia = @Provincia;";

        private const string SqlDistritos = @"
                    SELECT distrito AS item,
                           descripcion
                    FROM dbo.distritos
                    WHERE Provincia = @Provincia
                      AND Canton = @Canton;";

        private const string SqlProvinciaExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.Provincias
                    WHERE Provincia = @Provincia;";

        private const string SqlProvinciaInsert = @"
                    INSERT INTO dbo.provincias
                    (
                        provincia,
                        descripcion,
                        COD_PAIS,
                        ACTIVO,
                        REGISTRO_USUARIO,
                        REGISTRO_FECHA
                    )
                    VALUES
                    (
                        @Provincia,
                        @Descripcion,
                        'CRC',
                        1,
                        @Usuario,
                        GETDATE()
                    );";

        private const string SqlProvinciaUpdate = @"
                    UPDATE dbo.provincias
                    SET descripcion = @Descripcion
                    WHERE Provincia = @Provincia;";

        private const string SqlCantonExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.Cantones
                    WHERE Provincia = @Provincia
                      AND Canton = @Canton;";

        private const string SqlCantonInsert = @"
                    INSERT INTO dbo.cantones
                    (
                        provincia,
                        canton,
                        descripcion,
                        COD_PAIS,
                        ACTIVO,
                        REGISTRO_USUARIO,
                        REGISTRO_FECHA
                    )
                    VALUES
                    (
                        @Provincia,
                        @Canton,
                        @Descripcion,
                        'CRC',
                        1,
                        @Usuario,
                        GETDATE()
                    );";

        private const string SqlCantonUpdate = @"
                    UPDATE dbo.cantones
                    SET descripcion = @Descripcion
                    WHERE Provincia = @Provincia
                      AND Canton = @Canton;";

        private const string SqlDistritoExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.distritos
                    WHERE Provincia = @Provincia
                      AND Canton = @Canton
                      AND distrito = @Distrito;";

        private const string SqlDistritoInsert = @"
                    INSERT INTO dbo.distritos
                    (
                        provincia,
                        canton,
                        distrito,
                        descripcion,
                        COD_PAIS,
                        ACTIVO,
                        REGISTRO_USUARIO,
                        REGISTRO_FECHA
                    )
                    VALUES
                    (
                        @Provincia,
                        @Canton,
                        @Distrito,
                        @Descripcion,
                        'CRC',
                        1,
                        @Usuario,
                        GETDATE()
                    );";

        private const string SqlDistritoUpdate = @"
                    UPDATE dbo.distritos
                    SET descripcion = @Descripcion
                    WHERE Provincia = @Provincia
                      AND Canton = @Canton
                      AND Distrito = @Distrito;";

        public FrmAFDistribucionPoliticaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtiene y aplica la máscara numérica de cantón o distrito.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Tipo">Tipo de máscara: C para cantón, D para distrito.</param>
        /// <param name="Valor">Valor a normalizar.</param>
        /// <returns>Valor con ceros a la izquierda según la longitud configurada.</returns>
        public string AF_DistribucionPolitica_Mascara_Obtener(int CodEmpresa, string Tipo, string Valor)
        {
            var valorSeguro = NormalizarTexto(Valor);
            var sql = NormalizarTexto(Tipo).ToUpperInvariant() switch
            {
                "C" => SqlMascaraCanton,
                "D" => SqlMascaraDistrito,
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(sql))
            {
                return valorSeguro;
            }

            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                CodEmpresa,
                sql,
                0);

            if (result.Code != 0 || result.Result <= 0)
            {
                return valorSeguro;
            }

            return int.TryParse(valorSeguro, out int numero)
                ? numero.ToString($"D{result.Result}")
                : valorSeguro.PadLeft(result.Result, '0');
        }


        /// <summary>
        /// Obtiene la lista de provincias.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de provincias.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Provincias_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlProvincias);
        }


        /// <summary>
        /// Obtiene la lista de cantones por provincia.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Provincia">Código de provincia.</param>
        /// <returns>Listado de cantones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Cantones_Obtener(int CodEmpresa, string Provincia)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlCantones,
                new { Provincia = NormalizarTexto(Provincia) });
        }


        /// <summary>
        /// Obtiene la lista de distritos por provincia y cantón.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Provincia">Código de provincia.</param>
        /// <param name="Canton">Código de cantón.</param>
        /// <returns>Listado de distritos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Distritos_Obtener(int CodEmpresa, string Provincia, string Canton)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlDistritos,
                new
                {
                    Provincia = NormalizarTexto(Provincia),
                    Canton = NormalizarTexto(Canton)
                });
        }


        /// <summary>
        /// Inserta o actualiza una provincia, cantón o distrito según el tipo indicado.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="Info">Datos de distribución política.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_DistribucionPolitica_Guardar(int CodEmpresa, string Usuario, AfDistribucionesDto Info)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos de distribución política son requeridos.", -2);
            }

            var tipoSeguro = NormalizarTexto(Info.tipo).ToUpperInvariant();
            var result = tipoSeguro switch
            {
                "P" => GuardarProvincia(CodEmpresa, Usuario, Info),
                "C" => GuardarCanton(CodEmpresa, Usuario, Info),
                "D" => GuardarDistrito(CodEmpresa, Usuario, Info),
                _ => DbHelper.ErrorResponse("Tipo de distribución política no válido.", -2)
            };

            return result;
        }


        /// <summary>
        /// Guarda una provincia nueva o actualiza la existente.
        /// </summary>
        private ErrorDto GuardarProvincia(int codEmpresa, string usuario, AfDistribucionesDto info)
        {
            var parametros = new
            {
                Provincia = NormalizarTexto(info.codigo),
                Descripcion = NormalizarTexto(info.descripcion),
                Usuario = NormalizarTexto(usuario)
            };

            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                var existe = connection.QueryFirstOrDefault<int>(SqlProvinciaExiste, parametros);
                connection.Execute(existe == 0 ? SqlProvinciaInsert : SqlProvinciaUpdate, parametros);
                RegistrarBitacora(codEmpresa, usuario, $"Provincia: {parametros.Descripcion}", existe == 0);
                return true;
            });

            return CrearRespuestaGuardado(result);
        }


        /// <summary>
        /// Guarda un cantón nuevo o actualiza el existente.
        /// </summary>
        private ErrorDto GuardarCanton(int codEmpresa, string usuario, AfDistribucionesDto info)
        {
            var parametros = new
            {
                Provincia = NormalizarTexto(info.provincia),
                Canton = NormalizarTexto(info.codigo),
                Descripcion = NormalizarTexto(info.descripcion),
                Usuario = NormalizarTexto(usuario)
            };

            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                var existe = connection.QueryFirstOrDefault<int>(SqlCantonExiste, parametros);
                connection.Execute(existe == 0 ? SqlCantonInsert : SqlCantonUpdate, parametros);
                RegistrarBitacora(codEmpresa, usuario, $"Prov: {parametros.Provincia} Canton:{parametros.Descripcion}", existe == 0);
                return true;
            });

            return CrearRespuestaGuardado(result);
        }


        /// <summary>
        /// Guarda un distrito nuevo o actualiza el existente.
        /// </summary>
        private ErrorDto GuardarDistrito(int codEmpresa, string usuario, AfDistribucionesDto info)
        {
            var cantonMascara = AF_DistribucionPolitica_Mascara_Obtener(codEmpresa, "C", NormalizarTexto(info.canton));
            var distritoMascara = AF_DistribucionPolitica_Mascara_Obtener(codEmpresa, "D", NormalizarTexto(info.codigo));
            var parametros = new
            {
                Provincia = NormalizarTexto(info.provincia),
                Canton = cantonMascara,
                Distrito = distritoMascara,
                Descripcion = NormalizarTexto(info.descripcion),
                Usuario = NormalizarTexto(usuario)
            };

            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                var existe = connection.QueryFirstOrDefault<int>(SqlDistritoExiste, parametros);
                connection.Execute(existe == 0 ? SqlDistritoInsert : SqlDistritoUpdate, parametros);
                RegistrarBitacora(codEmpresa, usuario, $"Prov: {parametros.Provincia} Cant:{NormalizarTexto(info.canton)} Dist:{parametros.Descripcion}", existe == 0);
                return true;
            });

            return CrearRespuestaGuardado(result);
        }


        /// <summary>
        /// Crea una respuesta estándar para las operaciones de guardado.
        /// </summary>
        private static ErrorDto CrearRespuestaGuardado(ErrorDto<bool> result)
        {
            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar distribución política.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Registra la operación en bitácora.
        /// </summary>
        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, bool esNuevo)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = detalle,
                Movimiento = esNuevo ? "Registra - WEB" : "Modifica - WEB",
                Modulo = 9
            });
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