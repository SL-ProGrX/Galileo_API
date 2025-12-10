using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;
using System.Text.RegularExpressions;

namespace Galileo.DataBaseTier
{
    public partial class MProGrxMain
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MProGrXAuxiliarDB _AuxiliarDB;
        private const string connectionStringName = "DefaultConnString";

        public MProGrxMain(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
            _AuxiliarDB = new MProGrXAuxiliarDB(_config);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCardNumer"></param>
        /// <returns></returns>
        public static string FxTarjetaTipo(string pCardNumer) // cambiar a angular
        {
            string vResultado = "Tarjeta Inválida";
            bool bDetected = false;

            if (pCardNumer.Substring(0, 1) == "4")
            {
                vResultado = "VISA";
                bDetected = true;
            }

            if (!bDetected)
            {
                switch (pCardNumer.Substring(0, 2))
                {
                    case "51":
                    case "52":
                    case "53":
                    case "54":
                    case "55":
                        vResultado = "MasterCard";
                        bDetected = true;
                        break;
                }
            }

            if (pCardNumer.Substring(0, 4) == "6011" && !bDetected)
            {
                vResultado = "Discover";
                bDetected = true;
            }

            if (!bDetected)
            {
                switch (pCardNumer.Substring(0, 2))
                {
                    case "34":
                    case "37":
                        vResultado = "American Express";
                        break;
                }
            }

            return vResultado;
        }

        /// <summary>
        /// Validar tarjeta de crédito con el algoritmo de Luhn
        /// </summary>
        /// <param name="pCardNumer"></param>
        /// <returns></returns>
        public static bool FxTarjetaValida(string pCardNumer) // cambiar a angular
        {
            bool vResultado = false;
            int vNum = 0;
            bool vPar = true;
            string vCadenaX = "";
            string vCadenaY = "";
            string vCadenaZ = "";
            string vTrans = "";
            int vCardLargo = 0;

            if (pCardNumer.Length < 15 || pCardNumer.Length > 16)
            {
                return false;
            }

            vCardLargo = pCardNumer.Length;
            vCadenaX = pCardNumer.Substring(0, vCardLargo - 1);
            vCadenaY = pCardNumer.Substring(vCardLargo - 1);
            var sbCadenaZ = new System.Text.StringBuilder();

            // Algoritmo de Luhn
            for (int i = 1; i <= vCardLargo - 1; i++)
            {
                vNum = Convert.ToInt32(vCadenaX.Substring(vCardLargo - i - 1, 1));

                if (vPar)
                {
                    vNum += vNum;
                    vTrans = Convert.ToString(vNum);
                    if (vTrans.Length == 2)
                    {
                        vNum = Convert.ToInt32(vTrans.Substring(0, 1)) + Convert.ToInt32(vTrans.Substring(1, 1));
                    }
                }

                vPar = !vPar;

                sbCadenaZ.Insert(0, vNum);
            }
            vCadenaZ = sbCadenaZ.ToString();

            vNum = 0;
            // Suma números paso 2
            for (int i = 0; i < vCadenaZ.Length; i++)
            {
                vNum += Convert.ToInt32(vCadenaZ.Substring(i, 1));
            }

            vNum *= 9;
            vCadenaZ = vNum.ToString().Substring(vNum.ToString().Length - 1);

            if (vCadenaY == vCadenaZ)
            {
                vResultado = true;
            }

            return vResultado;
        }

        /// <summary>
        /// Consulta el estado de un cliente en el sistema
        /// </summary>
        /// <param name="pCedula"></param>
        /// <param name="pUsuario"></param>
        /// <returns></returns>
        public List<ConsultaStatusResultDto> DatosObtener(string pCedula, string pUsuario)
        {
            List<ConsultaStatusResultDto> resp;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var procedure = "[spSYS_RA_Consulta_Status]";
                    var values = new
                    {
                        Cedula = pCedula,
                        Usuario = pUsuario
                    };

                    resp = connection.Query<ConsultaStatusResultDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                resp = new List<ConsultaStatusResultDto>();
                _ = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Convierte el tipo de cambio a la moneda del sistema
        /// </summary>
        /// <param name="pTipoCambio"></param>
        /// <returns></returns>
        public static double fxSys_Tipo_Cambio_Apl(decimal pTipoCambio)
        {
            double resultado = 1;

            if (pTipoCambio == 0)
            {
                pTipoCambio = 1;
            }

            if (pTipoCambio > 0)
            {
                resultado = Convert.ToDouble(pTipoCambio);
            }
            else
            {
                resultado = 1 / Math.Abs(Convert.ToDouble(pTipoCambio));
            }

            return resultado;
        }

        public string FxSIFParametros(int CodEmpresa, string pCodigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string resultado = "";

            using (var connection = new SqlConnection(stringConn))
            {
                string query = "SELECT valor FROM SIF_parametros WHERE cod_parametro = @Codigo";
                resultado = connection.QueryFirstOrDefault<string>(query, new { Codigo = pCodigo }) ?? string.Empty;
            }

            return resultado ?? ""; // Si no se encuentra un valor, se retorna una cadena vacía
        }

        public ErrorDto SbSIFRegistraTags(SifRegistraTagsRequestDto req)
        {
            ErrorDto result = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "spSIFRegistraTags";
                    var parameters = new
                    {
                        Codigo = req.Codigo,
                        Tag = req.Tag,
                        Usuario = req.Usuario,
                        Observacion = req.Observacion,
                        Documento = req.Documento,
                        Modulo = req.Modulo,
                        Llave_01 = req.Llave_01,
                        Llave_02 = req.Llave_02,
                        Llave_03 = req.Llave_03
                    };

                    connection.Execute(procedure, parameters, commandType: CommandType.StoredProcedure);
                    result.Code = 0;
                    result.Description = "ok";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        public List<MenuUsoResultDto> SbSIFRegistraTags(MenuUsoRequestDto req)
        {
            List<MenuUsoResultDto> result = new List<MenuUsoResultDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    string query = "select *,dbo.fxSEG_MenuAccess(" + req.Empresa_Id + "," + req.Usuario + "," + req.Modulo + "," + req.Formulario + "," + req.Tipo + ") as Acceso FROM SIF_parametros WHERE cod_parametro = @Codigo";
                    ParametroDto? resultado = connection.QueryFirstOrDefault<ParametroDto>(query);

                    if (resultado != null)
                    {
                        var procedure = "spSEG_MenuUsos";
                        var parameters = new
                        {
                            menu_nodo = req.menu_nodo,
                            Empresa_Id = req.Empresa_Id,
                            Usuario = req.Usuario
                        };

                        result = connection.Query<MenuUsoResultDto>(procedure, parameters, commandType: CommandType.StoredProcedure).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                result = new List<MenuUsoResultDto>();
            }
            return result;
        }

        public List<MenuFavoritosResultDto> SbSIFRegistraTags(MenuFavoritosRequestDto req)
        {
            List<MenuFavoritosResultDto> result;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "spSEG_MenuFavoritos";
                    var parameters = new
                    {
                        Empresa_Id = req.Empresa_Id,
                        Usuario = req.Usuario
                    };

                    result = connection.Query<MenuFavoritosResultDto>(procedure, parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception)
            {
                result = new List<MenuFavoritosResultDto>();
            }
            return result;
        }

        public string sbMenuSeguridad(string usuario)
        {
            string result = "";
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    string query = "select O.modulo"
                                   + " from permisos P inner join  opciones O on P.id_opt = O.id_opt"
                                   + " and P.tipo = 'U' and P.nombre in(select userID from usuarios where nombre = '" + usuario + "')"
                                   + " group by O.modulo"
                                   + " Union "
                                   + " select O.modulo"
                                   + " from permisos P inner join  opciones O on P.id_opt = O.id_opt"
                                   + " and P.tipo = 'G' and P.nombre in(select id_Grupo from miembros where nombre = '" + usuario + "')"
                                   + " group by O.modulo";
                    result = connection.QueryFirstOrDefault<string>(query) ?? string.Empty;

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                result = "";
            }
            return result;
        }

        public string sbCargaCbo(string vTipo, string vFiltro)
        {
            string result = "";

            string strSQL = "";
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    switch (vTipo)
                    {
                        case "INSTITUCIONES":
                            strSQL = "select cod_institucion as xLlave,descripcion as xDesc from instituciones";
                            break;
                        case "PROMOTORES":
                            strSQL = "select ID_PROMOTOR as xLlave,Nombre as xDesc from promotores";
                            break;
                        case "PROFESIONES":
                            strSQL = "select COD_PROFESION as xLlave,descripcion as xDesc from AFI_PROFESIONES";
                            break;
                        case "SECTORES":
                            strSQL = "select COD_SECTOR as xLlave,descripcion as xDesc from AFI_SECTORES";
                            break;
                        case "BANCOS":
                            strSQL = "select id_banco as xLlave,descripcion as xDesc from Tes_Bancos";
                            break;
                        case "PROVINCIAS":
                            strSQL = "select provincia as xLlave,descripcion as xDesc from provincias";
                            break;
                        case "CANTONES":
                            strSQL = "select canton as xLlave,descripcion as xDesc from cantones";
                            break;
                        case "DISTRITOS":
                            strSQL = "select distrito as xLlave,descripcion as xDesc from distritos";
                            break;
                    }
                    result = connection.QueryFirstOrDefault<string>(strSQL) ?? string.Empty;

                }
            }
            catch (Exception)
            {
                result = "";
            }
            return result;
        }

        public static ErrorDto SbToolBarRead()
        {
            ErrorDto result = new ErrorDto();
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "meToolBar.ini");
            string toolBarValue = "00";

            try
            {
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, toolBarValue);
                }
                File.ReadAllText(filePath);
                result.Code = 1;
                result.Description = "ok";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        public string ProGrX_Theme(string usuario)
        {
            string result = "";
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "spSEG_Usuario_Theme";
                    var parameters = new
                    {
                        Usuario = usuario
                    };
                    result = connection.Query<string>(procedure, parameters, commandType: CommandType.StoredProcedure).FirstOrDefault() ?? "";
                }
            }
            catch (Exception)
            {
                result = "";
            }
            return result;
        }

        public List<EmpresaEnlaceResultDto> EmpresaEnlaceObtener()
        {
            List<EmpresaEnlaceResultDto> result;

            string strSQL = "";
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    strSQL = $@"select 
                                    cod_empresa_enlace,
                                    Nombre,
                                    SysCrdPlanPago,
                                    SysDocVersion,
                                    SysTesVersion, 
                                    SYS_CCSS_IND,
                                    ec_visible_patrimonio,
                                    ec_visible_fondos,
                                    ec_visible_creditos,
                                    ec_visible_fianzas,
                                    estadoCuenta
                              from dbo.sif_empresa";
                    result = connection.Query<EmpresaEnlaceResultDto>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                result = new List<EmpresaEnlaceResultDto>();
            }
            return result;
        }

        public List<SifOficinasUsuarioResultDto> CargaOficinas(string usuario, int codEmpresa)
        {
            List<SifOficinasUsuarioResultDto> result;
            try
            {
                string stringConn = _config.GetConnectionString(connectionStringName) ?? string.Empty;
                if (string.IsNullOrEmpty(stringConn))
                {
                    throw new InvalidOperationException("Connection string cannot be null or empty.");
                }
                if (codEmpresa != 0)
                {
                    stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa) ?? string.Empty;
                    if (string.IsNullOrEmpty(stringConn))
                    {
                        throw new InvalidOperationException("Empresa connection string cannot be null or empty.");
                    }
                }
                using (var connection = new SqlConnection(stringConn))
                {
                    var procedure = "sbSIFOficinasUsuario";
                    var parameters = new
                    {
                        Usuario = usuario
                    };
                    result = connection.Query<SifOficinasUsuarioResultDto>(procedure, parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception)
            {
                result = new List<SifOficinasUsuarioResultDto>();
            }
            return result;
        }

        public decimal glngFechaCR(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ParAhcr? par_ahcr;
            decimal glngFechaCR = 0;

            try
            {
                using (var connection = new SqlConnection(stringConn))
                {
                    var queryPar_Ahcr = "Select *,Getdate() as FechaAlterna from par_ahcr";
                    par_ahcr = connection.Query<ParAhcr>(queryPar_Ahcr).FirstOrDefault();
                    if (par_ahcr != null)
                    {
                        DateTime vFecha = par_ahcr.cr_fecha_calculo.GetValueOrDefault(par_ahcr.fechaalterna);
                        int year = vFecha.Year;
                        int month = vFecha.Month;
                        string fechaStr = year.ToString() + month.ToString("00");
                        glngFechaCR = decimal.Parse(fechaStr);
                    }
                    else
                    {
                        glngFechaCR = 0;
                    }
                }
            }
            catch (Exception)
            {
                glngFechaCR = 0;
            }
            return glngFechaCR;
        }

        public static partial class SeguridadSqlHelper
        {
            [GeneratedRegex(@"(?i)\b(SELECT|DELETE|UPDATE|INSERT|EXEC|DROP|CREATE|ALTER)\b|sp_|'", RegexOptions.Compiled)]
            private static partial Regex PatronPeligrosoRegex();

            public static bool ContieneSqlPeligroso(string texto)
            {
                return PatronPeligrosoRegex().IsMatch(texto);
            }
        }

        public ErrorDto<bool> fxSIFValidaCadena(string pCadena)
        {
            var error = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };

            if (string.IsNullOrEmpty(pCadena))
            {
                return error;
            }

            if (SeguridadSqlHelper.ContieneSqlPeligroso(pCadena))
            {
                error.Code = -1;
                error.Description = "!Error: El criterio de busqueda contiene información o datos que pueden afectar potencialmente la integridad de la información..!";
                error.Result = false;
            }
            return error;
        }

        public ErrorDto<bool> fxSys_RA_Consulta(int CodEmpresa, string pCedula, string pUsuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };
            try
            {

                using var connection = new SqlConnection(stringConn);

                var query = $@"exec spSYS_RA_Consulta_Status @cedula , @usuario";
                var result = connection.Query<ConsultaStatusResultDto>(query, new { cedula = pCedula, usuario = pUsuario }).FirstOrDefault();

                if (result != null && result.PERSONA_ID > 0 && result.AUTORIZACION_ID == 0)
                {
                    response.Result = false;
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }
            return response;
        }

        public ErrorDto sbEstadoCuenta_Email_Corte(int CodEmpresa, string pUsuario, string vCedula, string vEmail, DateTime? vCorte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                if (string.IsNullOrEmpty(vCedula))
                {
                    response.Code = -1;
                    response.Description = "Especifique la Identificación de la Persona";
                    return response;
                }

                if (string.IsNullOrEmpty(vEmail))
                {
                    response.Code = -1;
                    response.Description = "Especifique un Correo Electrónico Válido!";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                string? vFechaCorteNullable = _AuxiliarDB.validaFechaGlobal(vCorte);
                string vFechaCorte = vFechaCorteNullable ?? string.Empty;

                var query = $@"exec spSys_Estado_Cuenta_Corte @cedula , @corte, @email,@usuario ";
                response.Code = connection.Query(query, new { cedula = vCedula, corte = vFechaCorte, email = vEmail, usuario = pUsuario }).FirstOrDefault();


                // You can use queryResult here if needed

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = pUsuario,
                    DetalleMovimiento = $"Estado de Cuenta {vCedula}, email: {vEmail}, Corte: {vCorte} ",
                    Movimiento = "Aplica - WEB",
                    Modulo = 10
                });

                response.Description = "Estado de Cuenta enviado al Correo Electrónico registrado de la persona!";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

    }

}
