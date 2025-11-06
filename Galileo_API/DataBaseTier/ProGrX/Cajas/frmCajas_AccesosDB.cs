using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Cajas;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCajas_AccesosDB
    {
        private readonly IConfiguration _config;
        private readonly MTesoreria _mtes;

        public frmCajas_AccesosDB(IConfiguration config)
        {
            _config = config;
            _mtes = new MTesoreria(_config);
        }

        /// <summary>
        /// Obtiene cajas disponibles
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Apertura_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "spCajas_CierreCajasDisponibles";

                    response.Result = connection.Query(query, new { Usuario = usuario }, commandType: CommandType.StoredProcedure)
                        .Select(row => new DropDownListaGenericaModel
                        {
                            item = row.IdX,
                            descripcion = row.ItmX
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Abre caja para el usuario
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="usuario"></param>
        /// <param name="appVersion"></param>
        /// <param name="clave"></param>
        /// <returns></returns>
        public ErrorDto Cajas_AbreCaja(int codEmpresa, string codCaja, string usuario, string appVersion, string clave)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            string claveCifrada = FxStringCifrado(clave);

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",

            };

            try
            {
                using var connection = new SqlConnection(stringConn);


                var sqlValidar = @"
                            SELECT COUNT(*) 
                            FROM cajas_usuarios 
                            WHERE usuario = @Usuario 
                              AND contrasena = @ClaveCifrada
                              AND cod_caja = @CodCaja";

                int aceptado = connection.ExecuteScalar<int>(sqlValidar, new
                {
                    Usuario = usuario,
                    ClaveCifrada = claveCifrada,
                    CodCaja = codCaja
                });

                if (aceptado <= 0)
                {
                    response.Code = -1;
                    response.Description = "No se encuentra autorizado para utilizar esta caja.";
                    return response;
                }


                var result = connection.QueryFirstOrDefault<CajasAperturaDto>("spCajas_AbreCaja",
                    new
                    {
                        Caja = codCaja,
                        Usuario = usuario,
                    },
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                {
                    response.Code = -1;
                    response.Description = "No existe Apertura Disponible para esta caja o se encuentra en uso por otro usuario.";
                    return response;
                }

                response.Description = "Ok";
                response.Code = 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }



        public static string FxStringCifrado(string input)
        {
            string vRes = "";
            string vResX = "";
            int vSec = 0;

            foreach (char c in input)
            {
                int ascii = (int)c;
                vRes = ascii.ToString() + vRes;
            }
            for (int i = 0; i < vRes.Length; i += 3)
            {
                int take = Math.Min(3, vRes.Length - i);
                string slice = vRes.Substring(i, take);
                int block = int.Parse(slice);
                int transformed = block;

                switch (vSec)
                {
                    case 0: transformed = block + 1; break;
                    case 1: transformed = block - 5; break;
                    case 2: transformed = block + 7; break;
                    case 3: transformed = block - 13; break;
                    case 4: transformed = block - 2; break;
                    case 5: transformed = block + 3; break;
                }

                vResX += transformed.ToString();
                vSec = (vSec + 1) % 6; 
            }

            return FxDepuraCadena(vResX);
        }

        public static string FxDepuraCadena(string cadena)
        {
            string vRes = "";

            for (int i = 0; i < cadena.Length - 1; i++)
            {
                string sub = cadena.Substring(i, 2);

                if (int.TryParse(sub, out int num))
                {
                    if (num > 31 && num != 39 && num != 34)
                    {
                        vRes = ((char)num) + vRes;
                    }
                }
            }

            return vRes;
        }



    }


}