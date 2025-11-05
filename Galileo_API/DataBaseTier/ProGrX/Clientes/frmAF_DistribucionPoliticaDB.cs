using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_DistribucionPoliticaDB
    {
        private readonly IConfiguration _config;
        private readonly mSecurityMainDb _mSecurity;

        public frmAF_DistribucionPoliticaDB(IConfiguration config)
        {
            _config = config;
            _mSecurity = new mSecurityMainDb(_config);
        }

        public ErrorDTO Bitacora(BitacoraInsertarDTO data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener mascara del canton o distrito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Tipo"></param>
        /// <param name="Valor"></param>
        /// <returns></returns>
        public string AF_DistribucionPolitica_Mascara_Obtener(int CodEmpresa, string Tipo, string Valor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string response = "";
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (Tipo == "C")
                    {
                        //Mascara del canton
                        query = "select MAX(LEN(canton)) as Caracteres from CANTONES";
                    } 
                    else if (Tipo == "D")
                    {
                        //Mascara del distrito
                        query = "select MAX(LEN(distrito)) as Caracteres from Distritos";
                    }
                    int vMascara = connection.QueryFirstOrDefault<int>(query);

                    if (int.TryParse(Valor, out int numero))
                    {
                        response = numero.ToString("D" + vMascara);
                    }
                    else
                    {
                        response = Valor.PadLeft(vMascara, '0');
                    }
                }
            }
            catch (Exception)
            {
                response = Valor;
            }

            return response;
        }

        /// <summary>
        /// Obtener lista de provincias
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Provincias_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select provincia as item, descripcion from provincias";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtener lista de cantones por provincia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Provincia"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Cantones_Obtener(int CodEmpresa, string Provincia)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select canton as item, descripcion from cantones where Provincia = @Provincia";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { Provincia }).ToList();
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
        /// Obtener lista de distritos por provincia y canton
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Provincia"></param>
        /// <param name="Canton"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Distritos_Obtener(int CodEmpresa, string Provincia, string Canton)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select distrito as item, descripcion from distritos where Provincia = @Provincia and Canton = @Canton";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { Provincia, Canton }).ToList();
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
        /// Guardar provincia, canton o distrito, dependiendo del tipo
        /// Insertar o actualizar segun si existe o no
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDTO AF_DistribucionPolitica_Guardar(int CodEmpresa, string Usuario, AF_DistribucionesDTO Info)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    switch(Info.tipo)
                    {
                        case "P":
                            query = "select isnull(count(*),0) as Existe from Provincias where Provincia = @Provincia";
                            int ExisteP = connection.QueryFirstOrDefault<int>(query, new { Provincia = Info.codigo });

                            if (ExisteP == 0) //Insertar
                            {
                                query = @"insert into provincias(provincia,descripcion, COD_PAIS, ACTIVO, REGISTRO_USUARIO, REGISTRO_FECHA) 
                                    values(@Provincia, @Descripcion, 'CRC', 1, @Usuario, GETDATE())";
                                connection.Execute(query, new
                                {
                                    Provincia = Info.codigo,
                                    Descripcion = Info.descripcion,
                                    Usuario
                                });

                                Bitacora(new BitacoraInsertarDTO
                                {
                                    EmpresaId = CodEmpresa,
                                    Usuario = Usuario.ToUpper(),
                                    DetalleMovimiento = "Provincia: " + Info.descripcion,
                                    Movimiento = "Registra - WEB",
                                    Modulo = 9
                                });
                            } 
                            else //Actualizar
                            {
                                query = "update provincias set descripcion = @Descripcion where Provincia = @Provincia";
                                connection.Execute(query, new
                                {
                                    Provincia = Info.codigo,
                                    Descripcion = Info.descripcion
                                });

                                Bitacora(new BitacoraInsertarDTO
                                {
                                    EmpresaId = CodEmpresa,
                                    Usuario = Usuario.ToUpper(),
                                    DetalleMovimiento = "Provincia: " + Info.descripcion,
                                    Movimiento = "Modifica - WEB",
                                    Modulo = 9
                                });
                            }
                            break;
                        case "C":
                            query = "select isnull(count(*),0) as Existe from Cantones where Provincia = @Provincia and Canton = @Canton";
                            int ExisteC = connection.QueryFirstOrDefault<int>(query, new { Provincia = Info.provincia, Canton = Info.codigo });

                            if (ExisteC == 0) //Insertar
                            {
                                query = @"insert into cantones(provincia,canton,descripcion, COD_PAIS, ACTIVO, REGISTRO_USUARIO, REGISTRO_FECHA) 
                                    values(@Provincia, @Canton, @Descripcion, 'CRC', 1, @Usuario, GETDATE())";
                                connection.Execute(query, new
                                {
                                    Provincia = Info.provincia,
                                    Canton = Info.codigo,
                                    Descripcion = Info.descripcion,
                                    Usuario
                                });

                                Bitacora(new BitacoraInsertarDTO
                                {
                                    EmpresaId = CodEmpresa,
                                    Usuario = Usuario.ToUpper(),
                                    DetalleMovimiento = "Prov: " + Info.provincia + " Canton:" + Info.descripcion,
                                    Movimiento = "Registra - WEB",
                                    Modulo = 9
                                });
                            }
                            else //Actualizar
                            {
                                query = "update cantones set descripcion = @Descripcion where Provincia = @Provincia and Canton = @Canton";
                                connection.Execute(query, new
                                {
                                    Provincia = Info.provincia,
                                    Canton = Info.codigo,
                                    Descripcion = Info.descripcion
                                });

                                Bitacora(new BitacoraInsertarDTO
                                {
                                    EmpresaId = CodEmpresa,
                                    Usuario = Usuario.ToUpper(),
                                    DetalleMovimiento = "Prov: " + Info.provincia + " Canton:" + Info.descripcion,
                                    Movimiento = "Modifica - WEB",
                                    Modulo = 9
                                });
                            }
                            break;
                        case "D":
                            string vCantonMascara = AF_DistribucionPolitica_Mascara_Obtener(CodEmpresa, "C", Info.canton);
                            string vDistritoMascara = AF_DistribucionPolitica_Mascara_Obtener(CodEmpresa, "D", Info.codigo);

                            query = "select isnull(count(*),0) as Existe from distritos where Provincia = @Provincia and Canton = @Canton and distrito = @Distrito";
                            int ExisteD = connection.QueryFirstOrDefault<int>(query, new { Provincia = Info.provincia, Canton = vCantonMascara, Distrito = vDistritoMascara });

                            if (ExisteD == 0) //Insertar
                            {
                                query = @"insert into distritos(provincia,canton,distrito,descripcion, COD_PAIS, ACTIVO, REGISTRO_USUARIO, REGISTRO_FECHA) 
                                    values(@Provincia, @Canton, @Distrito, @Descripcion, 'CRC', 1, @Usuario, GETDATE())";
                                connection.Execute(query, new
                                {
                                    Provincia = Info.provincia,
                                    Canton = vCantonMascara,
                                    Distrito = vDistritoMascara,
                                    Descripcion = Info.descripcion,
                                    Usuario
                                });

                                Bitacora(new BitacoraInsertarDTO
                                {
                                    EmpresaId = CodEmpresa,
                                    Usuario = Usuario.ToUpper(),
                                    DetalleMovimiento = "Prov: " + Info.provincia + " Cant:" + Info.canton + " Dist:" + Info.descripcion,
                                    Movimiento = "Registra - WEB",
                                    Modulo = 9
                                });
                            }
                            else //Actualizar
                            {
                                query = "update distritos set descripcion = @Descripcion where Provincia = @Provincia and Canton = @Canton and Distrito = @Distrito";
                                connection.Execute(query, new
                                {
                                    Provincia = Info.provincia,
                                    Canton = vCantonMascara,
                                    Distrito = vDistritoMascara,
                                    Descripcion = Info.descripcion
                                });

                                Bitacora(new BitacoraInsertarDTO
                                {
                                    EmpresaId = CodEmpresa,
                                    Usuario = Usuario.ToUpper(),
                                    DetalleMovimiento = "Prov: " + Info.provincia + " Cant:" + Info.canton + " Dist:" + Info.descripcion,
                                    Movimiento = "Modifica - WEB",
                                    Modulo = 9
                                });
                            }
                            break;
                    }
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
