using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Fondos;
using System.Diagnostics.Contracts;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier.ProGrX.Fondos
{
    public class frmFNDBeneficiarios_ContratosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 18; // Modulo de Fondo de Inversion
        private readonly mSecurityMainDb _Security_MainDB;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmFNDBeneficiarios_ContratosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Metodo para obtener la lista de beneficiarios de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDTO<List<FNDBeneficiarios_ContratosData>> FND_Beneficiarios_Contratos_Lista_Obtener(int CodEmpresa, string cedula, int operadora, string plan, long contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FNDBeneficiarios_ContratosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FNDBeneficiarios_ContratosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Select B.*, isnull(P.Descripcion,'') as 'Parentesco_Desc'
                                         from FND_CONTRATOS_BENEFICIARIOS B left join SYS_PARENTESCOS P on B.Parentesco = P.cod_Parentesco
                                         Where Cedula= @cedula
                                         and cod_Operadora = @operadora
                                         and cod_Plan = @plan
                                         and cod_Contrato = @contrato";
                    response.Result = connection.Query<FNDBeneficiarios_ContratosData>(query, new
                    {
                        cedula = cedula,
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
        public ErrorDTO<List<DropDownListaGenericaModel>> FND_Beneficiarios_Contratos_Parentescos_Obtener(int CodEmpresa)
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
        /// Metodo para guardar o actualizar un beneficiario de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDTO FND_Beneficiarios_Contratos_Guardar(int CodEmpresa, string usuario, FNDBeneficiarios_ContratosData data)
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
                        response = FNDBeneficiarios_Contratos_Insertar(CodEmpresa, usuario, data);
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

                        response = FNDBeneficiarios_Contratos_Actualizar(CodEmpresa, usuario, data);
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
        /// Metodo para validar la informacion del beneficiario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDTO<bool> fxValida(int CodEmpresa, FNDBeneficiarios_ContratosData data)
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
                                    from FND_CONTRATOS_BENEFICIARIOS
                                    where cedula = @cedula
                                    and cedulaBN = @cedulaBN 
                                    and consec <> @consec 
                                     and cod_Operadora = @operadora 
                                     and cod_Plan = @plan 
                                     and cod_Contrato = @contrato";
                    var existe = connection.QueryFirstOrDefault<int>(query, new
                    {
                        cedula = data.cedula,
                        cedulaBN = data.cedulabn,
                        consec = data.consec,
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

                    if (data.porcentaje <= 0)
                    {
                        response.Description += " - El porcentaje no es válido ...";
                    }
                    else
                    {
                        query = $@"select isnull(sum(porcentaje),0) as 'Porcentaje'
                                from FND_CONTRATOS_BENEFICIARIOS
                                where cedula = @cedula and consec <> @consec
                                 and cod_Operadora = @operadora
                                 and cod_Plan = @plan
                                 and cod_Contrato = @contrato";
                        var porcentaje = connection.QueryFirstOrDefault<decimal>(query, new
                        {
                            cedula = data.cedula,
                            consec = data.consec,
                            operadora = data.cod_operadora,
                            plan = data.cod_plan,
                            contrato = data.cod_contrato
                        });

                        if ((data.porcentaje + porcentaje) > 100)
                        {
                            response.Description += " - El porcentaje sobre pasa el total del 100% del total de los beneficiarios ...";
                        }

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
        /// Metodo para insertar un nuevo beneficiario de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDTO FNDBeneficiarios_Contratos_Insertar(int CodEmpresa, string usuario, FNDBeneficiarios_ContratosData data)
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

                    string fechaNac = _AuxiliarDB.validaFechaGlobal(data.fechanac);

                    var query = $@"INSERT INTO FND_CONTRATOS_BENEFICIARIOS
                                    (
                                        cedula,
                                        cedulaBN,
                                        nombre,
                                        parentesco,
                                        fechaNac,
                                        porcentaje,
                                        direccion,
                                        notas,
                                        telefono1,
                                        telefono2,
                                        email,
                                        apto_postal,
                                        cod_operadora,
                                        cod_plan,
                                        cod_contrato
                                    )
                                    VALUES
                                    (
                                        @cedula,
                                        @cedulaBN,
                                        @nombre,
                                        @parentesco,
                                        @fechaNac,
                                        @porcentaje,
                                        @direccion,
                                        @notas,
                                        @telefono1,
                                        @telefono2,
                                        @email,
                                        @apto_postal,
                                        @cod_operadora,
                                        @cod_plan,
                                        @cod_contrato
                                    );";
                    connection.Execute(query, new
                    {
                        cedula = data.cedula,
                        cedulaBN = data.cedulabn,
                        nombre = data.nombre.Trim().ToUpper() + " " + data.apellido1.Trim().ToUpper() + " " + data.apellido2.Trim().ToUpper(),
                        parentesco = data.parentesco,
                        fechaNac = fechaNac,
                        porcentaje = data.porcentaje,
                        direccion = data.direccion,
                        notas = data.notas,
                        telefono1 = data.telefono1,
                        telefono2 = data.telefono2,
                        email = data.email,
                        apto_postal = data.apto_postal,
                        cod_operadora = data.cod_operadora,
                        cod_plan = data.cod_plan,
                        cod_contrato = data.cod_contrato
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
        /// Metodo para actualizar un beneficiario de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private ErrorDTO FNDBeneficiarios_Contratos_Actualizar(int CodEmpresa, string usuario, FNDBeneficiarios_ContratosData data)
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
                    var query = $@"UPDATE FND_CONTRATOS_BENEFICIARIOS
                                        SET nombre = @nombre,
                                            cedulaBN = @cedulaBN,
                                            parentesco = @parentesco,
                                            notas = @notas,
                                            direccion = @direccion,
                                            apto_postal = @apto_postal,
                                            email = @email,
                                            telefono1 = @telefono1,
                                            telefono2 = @telefono2,
                                            fechaNac = @fechaNac,
                                            porcentaje = @porcentaje
                                        WHERE consec = @consec";
                    connection.Execute(query, new
                    {
                        nombre = data.nombre.Trim().ToUpper() + " " + data.apellido1.Trim().ToUpper() + " " + data.apellido2.Trim().ToUpper(),
                        cedulaBN = data.cedulabn,
                        parentesco = data.parentesco,
                        notas = data.notas,
                        direccion = data.direccion,
                        apto_postal = data.apto_postal,
                        email = data.email,
                        telefono1 = data.telefono1,
                        telefono2 = data.telefono2,
                        fechaNac = data.fechanac,
                        porcentaje = data.porcentaje,
                        consec = data.consec
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
        /// Metodo para eliminar un beneficiario de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consec"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO FNDBeneficiarios_Contratos_Borrar(int CodEmpresa, int consec, string usuario)
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
                    //Consulto la informacion del beneficiario
                    var query = $@"Select * from FND_CONTRATOS_BENEFICIARIOS where consec = @consec";
                    var existe = connection.QueryFirstOrDefault<FNDBeneficiarios_ContratosData>(query, new
                    {
                        consec = consec
                    });

                    query = $@"DELETE FROM FND_CONTRATOS_BENEFICIARIOS WHERE consec = @consec";
                    connection.Execute(query, new
                    {
                        consec = consec
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Beneficiario de Plan: Op. {existe.cod_operadora}..Pln:{existe.cod_plan}....Cnt:{existe.cod_contrato}.Id: {existe.consec} ",
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
        /// Metodo para obtener el consecutivo del beneficiario por cedula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <param name="operadora"></param>
        /// <returns></returns>
        public ErrorDTO<string> FNDBene_Cnt_CedulaBN_Obtener(int CodEmpresa, string cedula, string plan, long contrato, int operadora)
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
                    var query = $@"select isnull(count(*),0) + 1 as Consec from FND_CONTRATOS_BENEFICIARIOS where cedula = @cedula and cod_plan = @plan
                                        and cod_contrato = @contrato and cod_operadora = @operadora";
                    response.Result = connection.QueryFirstOrDefault<string>(query, new
                    {
                        cedula = cedula,
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
