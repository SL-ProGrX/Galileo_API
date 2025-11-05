using Dapper;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Credito;
using System.Data;
using System.Text.RegularExpressions;

namespace PgxAPI.DataBaseTier
{
    public class mProGrx_Main
    {
        private readonly IConfiguration _config;
        private readonly mSecurityMainDb _Security_MainDB;
        private mProGrX_AuxiliarDB _AuxiliarDB;

        public mProGrx_Main(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCardNumer"></param>
        /// <returns></returns>
        public string FxTarjetaTipo(string pCardNumer) // cambiar a angular
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
                        bDetected = true;
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
        public bool FxTarjetaValida(string pCardNumer) // cambiar a angular
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
            vCadenaZ = "";

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

                vCadenaZ = Convert.ToString(vNum) + vCadenaZ;
            }

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
        public List<ConsultaStatusResultDTO> DatosObtener(string pCedula, string pUsuario)
        {
            List<ConsultaStatusResultDTO> resp = new List<ConsultaStatusResultDTO>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {

                    var procedure = "[spSYS_RA_Consulta_Status]";
                    var values = new
                    {
                        Cedula = pCedula,
                        Usuario = pUsuario
                    };

                    resp = connection.Query<ConsultaStatusResultDTO>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                resp = null;
                _ = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Convierte el tipo de cambio a la moneda del sistema
        /// </summary>
        /// <param name="pTipoCambio"></param>
        /// <returns></returns>
        public double fxSys_Tipo_Cambio_Apl(decimal pTipoCambio)
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
                resultado = connection.QueryFirstOrDefault<string>(query, new { Codigo = pCodigo });
            }

            return resultado ?? ""; // Si no se encuentra un valor, se retorna una cadena vacía
        }

        public ErrorDTO SbSIFRegistraTags(SIFRegistraTagsRequestDTO req)
        {
            ErrorDTO result = new ErrorDTO();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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

        public List<MenuUsoResultDTO> SbSIFRegistraTags(MenuUsoRequestDTO req)
        {
            List<MenuUsoResultDTO> result = new List<MenuUsoResultDTO>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    string query = "select *,dbo.fxSEG_MenuAccess(" + req.Empresa_Id + "," + req.Usuario + "," + req.Modulo + "," + req.Formulario + "," + req.Tipo + ") as Acceso FROM SIF_parametros WHERE cod_parametro = @Codigo";
                    ParametroDTO resultado = connection.QueryFirstOrDefault<ParametroDTO>(query);

                    if (resultado != null)
                    {
                        var procedure = "spSEG_MenuUsos";
                        var parameters = new
                        {
                            menu_nodo = req.menu_nodo,
                            Empresa_Id = req.Empresa_Id,
                            Usuario = req.Usuario
                        };

                        result = connection.Query<MenuUsoResultDTO>(procedure, commandType: CommandType.StoredProcedure).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                result = null;
            }
            return result;
        }

        public List<MenuFavoritosResultDto> SbSIFRegistraTags(MenuFavoritosRequestDTO req)
        {
            List<MenuFavoritosResultDto> result = new List<MenuFavoritosResultDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "spSEG_MenuFavoritos";
                    var parameters = new
                    {
                        Empresa_Id = req.Empresa_Id,
                        Usuario = req.Usuario
                    };

                    result = connection.Query<MenuFavoritosResultDto>(procedure, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        public string sbMenuSeguridad(string usuario)
        {
            string result = "";
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                    result = connection.QueryFirstOrDefault<string>(query);

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                result = null;
            }
            return result;
        }

        public string sbCargaCbo(string vTipo, string vFiltro)
        {
            string result = "";

            string strSQL = "";
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                    result = connection.QueryFirstOrDefault<string>(strSQL);

                }
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        public ErrorDTO SbToolBarRead()
        {
            ErrorDTO result = new ErrorDTO();
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "meToolBar.ini");
            string toolBarValue = "00";

            try
            {
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, toolBarValue);
                }
                toolBarValue = File.ReadAllText(filePath);
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
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "spSEG_Usuario_Theme";
                    var parameters = new
                    {
                        Usuario = usuario
                    };
                    result = connection.Query<string>(procedure, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        public List<EmpresaEnlaceResultDTO> EmpresaEnlaceObtener()
        {
            List<EmpresaEnlaceResultDTO> result = new List<EmpresaEnlaceResultDTO>();

            string strSQL = "";
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
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
                    result = connection.Query<EmpresaEnlaceResultDTO>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        public List<SIFOficinasUsuarioResultDTO> CargaOficinas(string usuario, int codEmpresa)
        {
            List<SIFOficinasUsuarioResultDTO> result = new List<SIFOficinasUsuarioResultDTO>();
            try
            {
                string stringConn = _config.GetConnectionString("DefaultConnString");
                if (codEmpresa != 0)
                {
                    stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
                }
                using (var connection = new SqlConnection(stringConn))
                {
                    var procedure = "sbSIFOficinasUsuario";
                    var parameters = new
                    {
                        Usuario = usuario
                    };
                    result = connection.Query<SIFOficinasUsuarioResultDTO>(procedure, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        public decimal glngFechaCR(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            Par_Ahcr par_ahcr = new Par_Ahcr();
            decimal glngFechaCR = 0;

            try
            {
                using (var connection = new SqlConnection(stringConn))
                {
                    var queryPar_Ahcr = "Select *,Getdate() as FechaAlterna from par_ahcr";
                    par_ahcr = connection.Query<Par_Ahcr>(queryPar_Ahcr).FirstOrDefault();
                    DateTime vFecha = par_ahcr.cr_fecha_calculo.GetValueOrDefault(par_ahcr.fechaalterna);
                    int year = vFecha.Year;
                    int month = vFecha.Month;
                    string fechaStr = year.ToString() + month.ToString("00");
                    glngFechaCR = decimal.Parse(fechaStr);
                }
            }
            catch (Exception)
            {
                glngFechaCR = 0;
            }
            return glngFechaCR;
        }

        private static readonly Regex _patronPeligroso = new(
             @"(?i)\b(SELECT|DELETE|UPDATE|INSERT|EXEC|DROP|CREATE|ALTER)\b|sp_|'",
             RegexOptions.Compiled
         );

        public ErrorDTO<bool> fxSIFValidaCadena(string pCadena)
        {
            var error = new ErrorDTO<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };

            if (string.IsNullOrEmpty(pCadena))
            {
                return error;
            }

            if (_patronPeligroso.IsMatch(pCadena))
            {
                error.Code = -1;
                error.Description = "!Error: El criterio de busqueda contiene información o datos que pueden afectar potencialmente la integridad de la información..!";
                error.Result = false;
            }
            return error;
        }

        public ErrorDTO<bool> fxSys_RA_Consulta(int CodEmpresa, string pCedula, string pUsuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };
            try
            {
             
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSYS_RA_Consulta_Status @cedula , @usuario";
                    var result = connection.Query<ConsultaStatusResultDTO>(query, new { cedula = pCedula, usuario = pUsuario }).FirstOrDefault();

                    if(result.PERSONA_ID > 0 && result.AUTORIZACION_ID == 0)
                    {
                        response.Result = false;
                    }

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

        public ErrorDTO sbEstadoCuenta_Email_Corte(int CodEmpresa, string pUsuario, string vCedula, string vEmail,DateTime? vCorte )
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok",
            };
            try
            {
                if(string.IsNullOrEmpty(vCedula))
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
                {
                    string vFechaCorte = _AuxiliarDB.validaFechaGlobal(vCorte);

                    var query = $@"exec spSys_Estado_Cuenta_Corte @cedula , @corte, @email,@usuario ";
                    var result = connection.Query(query, new { cedula = vCedula, corte = vFechaCorte, email = vEmail, usuario = pUsuario }).FirstOrDefault();

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = pUsuario,
                        DetalleMovimiento = $"Estado de Cuenta {vCedula}, email: {vEmail}, Corte: {vCorte} ",
                        Movimiento = "Aplica - WEB",
                        Modulo = 10
                    });

                    response.Description = "Estado de Cuenta enviado al Correo Electrónico registrado de la persona!";

                }
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
