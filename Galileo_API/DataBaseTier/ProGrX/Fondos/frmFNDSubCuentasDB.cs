using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Fondos;

namespace PgxAPI.DataBaseTier.ProGrX.Fondos
{
    public class frmFNDSubCuentasDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 18; // Modulo de Fondo de Inversion
        private readonly mSecurityMainDb _Security_MainDB;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmFNDSubCuentasDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Metodo que obtiene la lista de SubCuentas de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDTO<List<FNDSubCuentasData>> FND_SubCuentas_Lista_Obtener(int CodEmpresa, int operadora, string plan, long contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FNDSubCuentasData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FNDSubCuentasData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Select B.*, isnull(P.Descripcion,'') as 'Parentesco_Desc'
                                     from FND_SubCUENTAS B left join SYS_PARENTESCOS P on B.Parentesco = P.cod_Parentesco
                                     Where 
                                      cod_Operadora = @operadora
                                      and cod_Plan = @plan 
                                      and cod_Contrato = @contrato";
                    response.Result = connection.Query<FNDSubCuentasData>(query, new
                    {
                        operadora = operadora,
                        plan = plan,
                        contrato = contrato
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
        /// Metodo para obtener la lista de parentescos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> FND_SubCuentas_Parentescos_Obtener(int CodEmpresa)
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
                    var query = $@"select rtrim(cod_Parentesco) as 'item', rtrim(Descripcion) as 'descripcion' from sys_Parentescos where activo = 1";
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
        /// Me
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDTO FND_SubCuentas_Guardar(int CodEmpresa, string usuario, FNDSubCuentasData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {


                    if (data.isNew)
                    {
                        ErrorDTO<bool> valida = fxValida(CodEmpresa, data);
                        if (valida.Code == -1)
                        {
                            response.Code = valida.Code;
                            response.Description = valida.Description;
                            return response;
                        }
                        response = FNDSubCuentas_Insertar(CodEmpresa, usuario, data);
                    }
                    else
                    {
                        ErrorDTO<bool> valida = fxValida(CodEmpresa, data);
                        if (valida.Code == -1)
                        {
                            response.Code = valida.Code;
                            response.Description = valida.Description;
                            return response;
                        }

                        response = FNDSubCuentas_Actualizar(CodEmpresa, usuario, data);
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

        /// <summary>
        /// Metodo para validar la informacion de la SubCuenta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDTO<bool> fxValida(int CodEmpresa, FNDSubCuentasData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<bool>
            {
                Code = 0,
                Description = "",
                Result = true
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Verifica que exista ningun otro Beneficiario con la misma cedula juridica
                    var query = $@"select isnull(count(*),0) as 'Existe'
                                    from FND_SubCUENTAS
                                    where cedula = @cedula
                                    and cedula = @cedulaBN 
                                    and IDX <> @consec 
                                     and cod_Operadora = @operadora 
                                     and cod_Plan = @plan 
                                     and cod_Contrato = @contrato";
                    var existe = connection.QueryFirstOrDefault<int>(query, new
                    {
                        cedula = data.cedula,
                        cedulaBN = data.cedula,
                        consec = data.idx,
                        operadora = data.cod_operadora,
                        plan = data.cod_plan,
                        contrato = data.cod_contrato
                    });

                    if (existe > 0)
                    {
                        response.Code = -1;
                        response.Description += " - Ya Existe ya un Beneficiario registrado con la mismo número de identificación ...";
                    }

                    if (data.parentesco == null)
                    {
                        response.Description += " - No se ha seleccionado ningún parentesco...";
                    }

                    if (data.nombre == null)
                    {
                        response.Description += " - Nombre del Beneficiario no es válido ...";
                    }

                    if (data.apellido1 == null)
                    {
                        response.Description += " - Nombre del Beneficiario no es válido ...";
                    }

                    if (data.apellido2 == null)
                    {
                        response.Description += " - Nombre del Beneficiario no es válido ...";
                    }

                    if (response.Description.Length > 0)
                    {
                        response.Code = -1;
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

        /// <summary>
        /// Metodo para insertar una SubCuenta de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDTO FNDSubCuentas_Insertar(int CodEmpresa, string usuario, FNDSubCuentasData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //saco consecutivo
                    var query = $@"select isnull(max(IDX),0) + 1 as ultimo from FND_SubCUENTAS  where cod_operadora = @operadora and cod_plan = @plan and cod_contrato = @contrato";
                    data.idx = connection.QueryFirstOrDefault<int>(query, new
                    {
                        operadora = data.cod_operadora,
                        plan = data.cod_plan,
                        contrato = data.cod_contrato
                    });

                    string fechaNac = _AuxiliarDB.validaFechaGlobal(data.fechanac);

                    query = $@"INSERT INTO FND_SubCUENTAS (
                                        IdX,
                                        cedula,
                                        Nombre,
                                        parentesco,
                                        fechaNac,
                                        cuota,
                                        APORTES,
                                        RENDIMIENTO,
                                        direccion,
                                        notas,
                                        telefono1,
                                        telefono2,
                                        email,
                                        apto_postal,
                                        cod_operadora,
                                        cod_plan,
                                        cod_contrato,
                                        cod_beneficiario,
                                        estado
                                    )
                                    VALUES (
                                        @IdX,
                                        @cedula,
                                        @Nombre,
                                        @parentesco,
                                        @fechaNac,
                                        @cuota,
                                        @APORTES,
                                        @RENDIMIENTO,
                                        @direccion,
                                        @notas,
                                        @telefono1,
                                        @telefono2,
                                        @email,
                                        @apto_postal,
                                        @cod_operadora,
                                        @cod_plan,
                                        @cod_contrato,
                                        @cod_beneficiario,
                                        @estado
                                    )";
                    connection.Execute(query, new
                    {
                        IdX = data.idx,
                        cedula = data.cedula,
                        Nombre = data.nombre.Trim().ToUpper() + " " + data.apellido1.Trim().ToUpper() + " " + data.apellido2.Trim().ToUpper(),
                        parentesco = data.parentesco,
                        fechaNac = fechaNac,
                        cuota = 0,
                        APORTES = 0,
                        RENDIMIENTO = 0,
                        direccion = data.direccion,
                        notas = data.notas,
                        telefono1 = data.telefono1,
                        telefono2 = data.telefono2,
                        email = data.email,
                        apto_postal = data.apto_postal,
                        cod_operadora = data.cod_operadora,
                        cod_plan = data.cod_plan,
                        cod_contrato = data.cod_contrato,
                        cod_beneficiario = 0,
                        estado = "A"
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Sub-Cuenta de Plan: Op. {data.cod_operadora}..Pln: {data.cod_plan}..Cnt:{data.cod_contrato}..Id:{data.idx}..Ced.{data.cedula}, Mnt.{data.cuota}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Metodo para actualizar una SubCuenta de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDTO FNDSubCuentas_Actualizar(int CodEmpresa, string usuario, FNDSubCuentasData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"UPDATE FND_SubCUENTAS
                                    SET
                                        nombre = @nombre,
                                        CEDULA = @cedula,
                                        parentesco = @parentesco,
                                        notas = @notas,
                                        direccion = @direccion,
                                        apto_postal = @apto_postal,
                                        email = @email,
                                        telefono1 = @telefono1,
                                        telefono2 = @telefono2,
                                        fechaNac = @fechaNac,
                                        cuota = @cuota
                                    WHERE
                                        IdX = @IdX";
                   
                    connection.Execute(query, new
                    {
                        IdX = data.idx,
                        nombre = data.nombre.Trim().ToUpper() + " " + data.apellido1.Trim().ToUpper() + " " + data.apellido2.Trim().ToUpper(),
                        cedula = data.cedula,
                        parentesco = data.parentesco,
                        notas = data.notas,
                        direccion = data.direccion,
                        apto_postal = data.apto_postal,
                        email = data.email,
                        telefono1 = data.telefono1,
                        telefono2 = data.telefono2,
                        fechaNac = _AuxiliarDB.validaFechaGlobal(data.fechanac),
                        cuota = data.cuota
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Sub-Cuenta de Plan: Op. {data.cod_operadora}..Pln: {data.cod_plan}..Cnt:{data.cod_contrato}..Id:{data.idx}..Ced.{data.cedula}, Mnt.{data.cuota}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Metodo para eliminar una SubCuenta de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consec"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO FNDSubCuentas_Borrar(int CodEmpresa, int consec, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Consulto la informacion de la sub cuenta
                    var query = $@"Select * from FND_SubCUENTAS where IDX = @consec";
                    var existe = connection.QueryFirstOrDefault<FNDSubCuentasData>(query, new
                    {
                        consec = consec
                    });

                    query = $@"delete FND_SubCUENTAS where IDX = @consec";
                    connection.Execute(query, new
                    {
                        consec = consec
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Sub-Cuenta de Plan: Op. {existe.cod_operadora}..Pln:{existe.cod_plan}....Cnt:{existe.cod_contrato}.Id: {existe.idx} ",
                        Movimiento = "Elimina - WEB",
                        Modulo = vModulo
                    });

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener el consecutivo de un beneficiario por cedula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <param name="operadora"></param>
        /// <returns></returns>
        public ErrorDTO<string> FNDDSubCuentas_Cedula_Obtener(int CodEmpresa, string plan, long contrato, int operadora)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(count(*),0) + 1 as IDX from FND_SubCUENTAS where cod_plan = @plan
                                        and cod_contrato = @contrato and cod_operadora = @operadora";
                    response.Result = connection.QueryFirstOrDefault<string>(query, new
                    {
                        plan = plan,
                        contrato = contrato,
                        operadora = operadora
                    });
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

    }
}
