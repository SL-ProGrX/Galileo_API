using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_InstitucionesDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;
        private readonly mCntLinkDB _mCntLink;

        public frmAF_InstitucionesDB(IConfiguration config)
        {
            _config = config;
            _mSecurity = new MSecurityMainDb(_config);
            _mCntLink = new mCntLinkDB(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener lista de instituciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Lista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select cod_institucion as item,descripcion from instituciones";
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
        /// Navegar al siguiente o anterior codigo de institución mediante el ScrollCode, según corresponda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ScrollCode"></param>
        /// <param name="CodInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<AfInstitucionDto> AF_Instituciones_Scroll_Obtener(int CodEmpresa, int ScrollCode, int CodInstitucion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AfInstitucionDto>
            {
                Code = 0,
                Result = new AfInstitucionDto()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Top 1 cod_institucion from instituciones";

                    if (ScrollCode == 1)
                    {
                        query += " where cod_institucion > @CodInstitucion order by cod_institucion asc";
                    }
                    else
                    {
                        query += " where cod_institucion < @CodInstitucion order by cod_institucion desc";
                    }
                    var Institucion = connection.Query<int>(query, new { CodInstitucion }).FirstOrDefault();
                    response = AF_Institucion_Obtener(CodEmpresa, Institucion);
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
        /// Obtener información de la institución mediante el código
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<AfInstitucionDto> AF_Institucion_Obtener(int CodEmpresa, int CodInstitucion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AfInstitucionDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new AfInstitucionDto()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select * from vAFI_Instituciones where cod_institucion = @CodInstitucion";
                    response.Result = connection.QueryFirstOrDefault<AfInstitucionDto>(query, new { CodInstitucion });
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
        /// Obtener lista de tipos de asientos, operadoras o divisas según corresponda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Tipo"></param>
        /// <param name="Conta"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_CargaCombo_Obtener(int CodEmpresa, string Tipo, int Conta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    switch(Tipo)
                    {
                        case "A": //Tipos de Asientos
                            query = "select rtrim(Tipo_Asiento) as item,rtrim(Tipo_Asiento) as descripcion from CntX_Tipos_Asientos where cod_contabilidad = @Conta";
                            break;
                        case "O": //Operadoras
                            //Carga Variables de los FONDOS
                            query = "SELECT COD_OPERADORA as item,rtrim(DESCRIPCION) as descripcion FROM FND_OPERADORAS";
                            break;
                        case "D": //Divisas
                            query = "SELECT rtrim(cod_Divisa) as item,rtrim(Descripcion) as descripcion FROM vSys_Divisas";
                            break;
                    }
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { Conta }).ToList();
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
        /// Obtener lista de planes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOperadora"></param>
        /// <param name="CodMoneda"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Planes_Obtener(int CodEmpresa, int CodOperadora, string CodMoneda)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            string filtro = "";
            try
            {
                if (CodOperadora != 0)
                {
                    filtro += "where cod_operadora = @CodOperadora";
                }
                if (!string.IsNullOrWhiteSpace(CodMoneda.Trim()))
                {
                    if (filtro == "")
                    {
                        filtro += "where cod_Moneda = @CodMoneda";

                    }
                    else
                    {
                        filtro += " and cod_Moneda = @CodMoneda";
                    }
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"select cod_plan as item, Descripcion from fnd_planes {filtro} order by COD_PLAN";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { CodOperadora, CodMoneda }).ToList();
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
        /// Obtener lista de empresas vinculadas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="Tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfInstitucionEmpresasDto>> AF_Institucion_Empresas_Obtener(int CodEmpresa, int CodInstitucion, int Tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfInstitucionEmpresasDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfInstitucionEmpresasDto>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spAFI_Institucion_Vinculadas @CodInstitucion, @Tipo";
                    response.Result = connection.Query<AfInstitucionEmpresasDto>(query, new { CodInstitucion, Tipo }).ToList();
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
        /// Obtener lista de codigos de deducción
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<AfInstitucionesCodigosDto>> AF_Instituciones_Codigos_Obtener(int CodEmpresa, int CodInstitucion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfInstitucionesCodigosDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfInstitucionesCodigosDto>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select COD_DEDUCCION,descripcion,activo, COD_INSTITUCION 
                        from AFI_INSTITUCIONES_CODIGOS 
                        WHERE COD_INSTITUCION = @CodInstitucion
                        order by COD_DEDUCCION";
                    response.Result = connection.Query<AfInstitucionesCodigosDto>(query, new { CodInstitucion }).ToList();
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
        /// Obtener listas de lineas vinculadas al codigo de deducción
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="Codigo"></param>
        /// <param name="rbCodigo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfInstitucionesCodigosLineasDto>> AF_Instituciones_Codigos_Lineas_Obtener(int CodEmpresa, int CodInstitucion, string Codigo, int rbCodigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfInstitucionesCodigosLineasDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfInstitucionesCodigosLineasDto>()
            };
            int? Estado = null;
            try
            {
                switch (rbCodigo)
                {
                    case 0:
                        Estado = null; //Todos
                        break;
                    case 1:
                        Estado = 1; //Activos
                        break;
                    case 2:
                        Estado = 0; //Inactivos
                        break;
                }
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"exec spAFI_Instituciones_Codigos_Lineas @CodInstitucion, @Codigo, @Estado";
                    response.Result = connection.Query<AfInstitucionesCodigosLineasDto>(query, new { CodInstitucion, Codigo, Estado }).ToList();
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
        /// Obtener lista de departamentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<AfInstitucionDepartamentosDto>> AF_Institucion_Departamentos_Obtener(int CodEmpresa, int CodInstitucion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfInstitucionDepartamentosDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfInstitucionDepartamentosDto>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spAFI_Institucion_Departamentos @CodInstitucion";
                    response.Result = connection.Query<AfInstitucionDepartamentosDto>(query, new { CodInstitucion }).ToList();
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
        /// Obtener lista de secciones asociadas a un departamento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="CodDepartamento"></param>
        /// <returns></returns>
        public ErrorDto<List<AfInstitucionSeccionesDto>> AF_Institucion_Secciones_Obtener(int CodEmpresa, int CodInstitucion, string CodDepartamento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AfInstitucionSeccionesDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfInstitucionSeccionesDto>()
            };
            try
            {
                if (CodDepartamento == "N/A")
                {
                    CodDepartamento = "";
                }
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spAFI_Institucion_Secciones @CodInstitucion, @CodDepartamento";
                    response.Result = connection.Query<AfInstitucionSeccionesDto>(query, new { CodInstitucion, CodDepartamento }).ToList();
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
        /// Cambiar fecha de corte de la institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="FechaCorte"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_CambiarFecha(int CodEmpresa, int CodInstitucion, string FechaCorte, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            // Validar que FechaCorte sea una fecha válida, sin importar el formato
            if (!DateTime.TryParse(FechaCorte, out DateTime fechaCorteDate))
            {
                response.Code = -1;
                response.Description = "La fecha ingresada no es válida.";
                return response;
            }

            // Asignar el formato yyyy/MM/dd a fechaCorteDate para la base de datos
            string fechaFormateada = fechaCorteDate.ToString("yyyy/MM/dd");

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "update instituciones set pr_fecha_corte = @FechaCorte where cod_institucion = @CodInstitucion";
                    connection.Execute(query, new { CodInstitucion, FechaCorte = fechaFormateada });
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Usuario.ToUpper(),
                    DetalleMovimiento = "Cambia Fecha de Corte Formalizaciones: " + fechaFormateada + " [Inst:" + CodInstitucion + "]",
                    Movimiento = "Aplica - WEB",
                    Modulo = 9
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Inicializar fecha de deducción de la institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="Proceso"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_InicializarDeduccion(int CodEmpresa, int CodInstitucion, string Proceso, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spPrm_Institucion_Proceso_Inicial @CodInstitucion, @Proceso, @Usuario";
                    connection.Execute(query, new { CodInstitucion, Proceso, Usuario });
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Usuario.ToUpper(),
                    DetalleMovimiento = "Inicializa Fecha Corte para Deducciones: " + Proceso + " [Inst:" + CodInstitucion + "]",
                    Movimiento = "Aplica - WEB",
                    Modulo = 9
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Guarda código, registra o actualiza según si existe o no
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Info"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Instituciones_Codigo_Guardar(int CodEmpresa, AfInstitucionesCodigosDto Info, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            var query = "";
            string movimiento = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryE = @"select isnull(count(*),0) as Existe from AFI_INSTITUCIONES_CODIGOS 
                        where COD_INSTITUCION = @CodInstitucion and COD_DEDUCCION = @CodDeduccion";
                    int Existe = connection.Query<int>(queryE, new { CodInstitucion = Info.cod_institucion, CodDeduccion = Info.cod_deduccion }).FirstOrDefault();

                    if (Existe == 0)
                    {
                        query = @"insert into AFI_INSTITUCIONES_CODIGOS(COD_INSTITUCION, COD_DEDUCCION,descripcion,activo,registro_fecha,registro_usuario) 
                            values(@CodInstitucion, @CodDeduccion, @Descripcion, @Activo, GETDATE(), @Usuario)";
                        movimiento = "Registra";
                    }
                    else
                    {
                        query = @"update AFI_INSTITUCIONES_CODIGOS set Descripcion = @Descripcion, activo = @Activo 
                            where COD_INSTITUCION = @CodInstitucion and cod_deduccion = @CodDeduccion";
                        movimiento = "Modifica";
                    }
                    connection.Execute(query, 
                        new { 
                            CodInstitucion = Info.cod_institucion, 
                            CodDeduccion = Info.cod_deduccion,
                            Descripcion = Info.descripcion,
                            Activo = Info.activo,
                            Usuario
                        }
                    );

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Cod. Deduc.: " + Info.cod_deduccion + " Inst.: " + Info.cod_institucion,
                        Movimiento = movimiento + " - WEB",
                        Modulo = 9
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
        /// Eliminar código asociado a la institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="CodDeduccion"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Instituciones_Codigo_Eliminar(int CodEmpresa, int CodInstitucion, string CodDeduccion, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "delete AFI_INSTITUCIONES_CODIGOS where COD_DEDUCCION = @CodDeduccion and cod_institucion = @CodInstitucion";
                    connection.Execute(query,
                        new
                        {
                            CodInstitucion,
                            CodDeduccion,
                            Usuario
                        }
                    );

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Cod. Deduc.: " + CodDeduccion+ " Inst.: " + CodInstitucion,
                        Movimiento = "Elimina - WEB",
                        Modulo = 9
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
        /// Guardar vinculación o desvinculación de lineas de un código de institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="CodDeduccion"></param>
        /// <param name="Codigo"></param>
        /// <param name="Checked"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Instituciones_Lineas_Asignacion_Guardar(int CodEmpresa, int CodInstitucion, string CodDeduccion, string Codigo, bool Checked, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            var query = "";
            string movimiento = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (Checked == true)
                    {
                        query = @"insert AFI_INSTITUCION_ASIGNACION(cod_institucion,cod_deduccion,codigo,registro_fecha,registro_usuario) 
                            values(@CodInstitucion, @CodDeduccion, @Codigo, GETDATE(), @Usuario)";
                        movimiento = "Registra";
                    }
                    else
                    {
                        query = @"delete AFI_INSTITUCION_ASIGNACION where cod_institucion = @CodInstitucion 
                            and cod_deduccion = @CodDeduccion and codigo = @Codigo";
                        movimiento = "Elimina";
                    }
                    connection.Execute(query, new { CodInstitucion, CodDeduccion, Codigo, Usuario });

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Inst. Asignación Código: "+CodDeduccion+" (Inst:"+CodInstitucion+") Línea Crd:" + Codigo,
                        Movimiento = movimiento + " - WEB",
                        Modulo = 9
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
        /// Guardar vinculación o desvinculación de empresa a la institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="CodDeductora"></param>
        /// <param name="Checked"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_Empresas_Guardar(int CodEmpresa, int CodInstitucion, int CodDeductora, bool Checked, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            var query = "";
            string movimiento = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (Checked == true)
                    {
                        query = @"insert AFI_INSTITUCION_DEDUCTORA(COD_INSTITUCION, COD_DEDUCTORA, REGISTRO_FECHA, REGISTRO_USUARIO) 
                                    values(@CodInstitucion, @CodDeductora, GETDATE(), @Usuario)";
                        movimiento = "Aplica";
                    }
                    else
                    {
                        query = "delete AFI_INSTITUCION_DEDUCTORA where cod_institucion = @CodInstitucion and cod_deductora = @CodDeductora";
                        movimiento = "Elimina";
                    }
                    connection.Execute(query, new { CodInstitucion, CodDeductora, Usuario });

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Institución : " + CodInstitucion + " -> Deductora: " + CodDeductora,
                        Movimiento = movimiento + " - WEB",
                        Modulo = 9
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
        /// Copiar institución, replica toda la información de una institución a una nueva
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="CopiaDesc"></param>
        /// <param name="CopiaDescCorta"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_Copiar(int CodEmpresa, int CodInstitucion, string CopiaDesc, string CopiaDescCorta, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"exec spAFI_Institucion_Copia @CodInstitucion,0, @CopiaDesc,
                            @CopiaDescCorta, @Usuario, 1, 1, 1, 1, 1, 1";
                    int CodInstDest = connection.QueryFirstOrDefault<int>(query, new { CodInstitucion, CopiaDesc, CopiaDescCorta, Usuario });

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Copia de Institución: " + CodInstitucion + " -> Nueva -> [Inst:" + CodInstDest + "]",
                        Movimiento = "Aplica - WEB",
                        Modulo = 9
                    }); 

                    response.Code = CodInstDest;
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
        /// Guardar información de la institución, ya sea nuevo o edición
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Info"></param>
        /// <param name="Usuario"></param>
        /// <param name="vEdita"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_Guardar(int CodEmpresa, AfInstitucionDto Info, string Usuario, bool vEdita)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            var query = "";
            try
            {
                string CtaCredito = _mCntLink.fxgCntCuentaFormato(CodEmpresa, false, Info.cta_crd_mask);
                string CtaObrero = _mCntLink.fxgCntCuentaFormato(CodEmpresa, false, Info.cta_obr_mask);
                string CtaPatronal = _mCntLink.fxgCntCuentaFormato(CodEmpresa, false, Info.cta_pat_mask);
                string CtaFondos = _mCntLink.fxgCntCuentaFormato(CodEmpresa, false, Info.cta_fnd_mask);
                string CtaInconsistencias = _mCntLink.fxgCntCuentaFormato(CodEmpresa, false, Info.cta_inc_mask);

                using var connection = new SqlConnection(stringConn);
                {
                    if(vEdita == true)
                    {
                        query = @"update instituciones set descripcion = @Descripcion, Desc_Corta = @DescCorta,
                        Activa = @Activa, Mora_Cierres =  @MoraAutomatica, DEDUCCION_PLANILLA =  @DeducionPlanilla,
                        planilla = @PlanillaRecibe, planilla_envio = @PlanillaEnvio, 
                        cod_divisa = @Divisa, FRECUENCIA = @TipoPago, direccion = @Direccion, 
                        cta_credito = @CtaCredito, cta_obrero = @CtaObrero, cta_patronal = @CtaPatronal, 
                        cta_fondos = @CtaFondos, cta_inconsistencia = @CtaInconsistencias, 
                        TipoAsiento = @TipoAsiento, codigo_aportes = @CodigoDeducAportes, codigo_creditos = @CodigoDeducCreditos,
                        codigo_aportes_env = @CodigoEnvAportes, codigo_creditos_env = @CodigoEnvCreditos, codigo_inst_deduc = @CodInstDeduc,
                        porc_ahorro = @PorcentajeAhorro, porc_aporte = @PorcentajeAporte, IncInclusiones = @MovInclusion, 
                        IncExclusiones = @MovExclusion, IncModificaciones = @MovModificacion, IncMantienen = @MovMantienen, 
                        pr_genera = @GeneraDeducciones, pr_carga = @CargaDeducciones, pr_desgloza = @Desgloza, 
                        pr_apAplica = @AhAplica, pr_apInco = @AhInconsistencias, pr_apDev = @AhDevoluciones, 
                        pr_crAplica = @CRAplica, pr_crInco = @CRInconsistencias, pr_crMora = @CRRecalculaMora, 
                        pr_cr_aplica_incon = @Inconsistencias, fnd_ap_aplica = @Devoluciones, fnd_cr_SOAplica = @FNDSocios, 
                        fnd_cr_ExAplica = @FNDExSocios, fnd_ap_plan = @DevPlan, fnd_ap_planp = @DevPlanPat, 
                        fnd_cr_soPlan = @PlanSocios, fnd_cr_exPlan = @PlanExSocios, fnd_ap_Operadora = @DevOp, 
                        fnd_cr_SoOperadora = @OPSocios,  fnd_cr_exOperadora = @OPExSocios, 
                        Compara_Indicador = @ChkCompara, compara_valor = @Compara, 
                        Historico_Cobro_Envio = @HistoricoCuotasEnviadas, Tipo_Cobro_Mora = @CuotasMora, 
                        TRANSITO_PLANILLAS_MES = @TransitoPlanillasMes, TRANSITO_COMPARA = @TransitoCompra 
                        where cod_institucion = @CodInstitucion";

                        connection.Execute(query, 
                            new {
                                CodInstitucion = Info.cod_institucion,
                                Descripcion = Info.descripcion.Trim(),
                                DescCorta = Info.desc_corta.Trim(),
                                Activa = Info.activa ? 1 : 0,
                                MoraAutomatica = Info.mora_cierres ? 1 : 0,
                                DeducionPlanilla = Info.deduccion_planilla,
                                PlanillaRecibe = Info.planilla,
                                PlanillaEnvio = Info.planilla_envio,
                                Divisa = Info.cod_divisa,
                                TipoPago = Info.frecuencia_id,
                                Direccion = Info.direccion,
                                CtaCredito,
                                CtaObrero,
                                CtaPatronal,
                                CtaFondos,
                                CtaInconsistencias,
                                TipoAsiento = Info.tipoasiento,
                                CodigoDeducAportes = Info.codigo_aportes,
                                CodigoDeducCreditos = Info.codigo_creditos,
                                CodigoEnvAportes = Info.codigo_aportes_env,
                                CodigoEnvCreditos = Info.codigo_creditos_env,
                                CodInstDeduc = Info.codigo_inst_deduc,
                                PorcentajeAhorro = Info.porc_ahorro,
                                PorcentajeAporte = Info.porc_aporte,
                                MovInclusion = Info.incinclusiones ? 1 : 0,
                                MovExclusion = Info.incexclusiones ? 1 : 0,
                                MovModificacion = Info.incmodificaciones ? 1 : 0,
                                MovMantienen = Info.incmantienen ? 1 : 0,
                                GeneraDeducciones = Info.pr_genera ? 1 : 0,
                                CargaDeducciones = Info.pr_carga ? 1 : 0,
                                Desgloza = Info.pr_desgloza ? 1 : 0,
                                AhAplica = Info.pr_apaplica ? 1 : 0,
                                AhInconsistencias = Info.pr_apinco ? 1 : 0,
                                AhDevoluciones = Info.pr_apdev ? 1 : 0,
                                CRAplica = Info.pr_craplica ? 1 : 0,
                                CRInconsistencias = Info.pr_crinco ? 1 : 0,
                                CRRecalculaMora = Info.pr_crmora ? 1 : 0,
                                Inconsistencias = Info.pr_cr_aplica_incon ? 1 : 0,
                                Devoluciones = Info.fnd_ap_aplica ? 1 : 0,
                                FNDSocios = Info.fnd_cr_soaplica ? 1 : 0,
                                FNDExSocios = Info.fnd_cr_exaplica ? 1 : 0,
                                DevPlan = Info.fnd_ap_plan,
                                DevPlanPat = Info.fnd_ap_planp,
                                PlanSocios = Info.fnd_cr_soplan,
                                PlanExSocios = Info.fnd_cr_explan,
                                DevOp = Info.fnd_ap_operadora,
                                OPSocios = Info.fnd_cr_sooperadora,
                                OPExSocios = Info.fnd_cr_exoperadora,
                                ChkCompara = Info.compara_indicador ? 1 : 0,
                                Compara = Info.compara_valor,
                                HistoricoCuotasEnviadas = Info.historico_cobro_envio,
                                CuotasMora = Info.tipo_cobro_mora,
                                TransitoPlanillasMes = Info.transito_planillas_mes,
                                TransitoCompra = Info.transito_compara
                            }
                        );

                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Institución No." + Info.cod_institucion,
                            Movimiento = "Modifica - WEB",
                            Modulo = 9
                        });
                    } else
                    {
                        query = @"insert into instituciones(descripcion,desc_Corta,activa, cod_divisa, mora_cierres 
                            ,DEDUCCION_PLANILLA,direccion,planilla,planilla_envio,cta_credito,cta_obrero
                            ,cta_patronal,cta_fondos,cta_inconsistencia,TipoAsiento,porc_ahorro,porc_aporte,pr_fecha_corte
                            ,pr_genera,pr_carga,pr_desgloza,pr_apAplica,pr_apDev,pr_apInco,pr_crAplica,pr_crInco
                            ,pr_crMora,pr_cr_aplica_incon,fnd_ap_aplica,fnd_ap_operadora,fnd_ap_plan 
                            ,fnd_ap_planp,fnd_cr_soAplica,fnd_cr_soOperadora,fnd_cr_soPlan,fnd_cr_exAplica,fnd_cr_exOperadora
                            ,fnd_cr_exPlan,codigo_aportes,codigo_creditos,codigo_aportes_env,codigo_creditos_env
                            ,IND_CAMBIA_FECPRO,compara_indicador,compara_valor,codigo_inst_deduc,Historico_Cobro_Envio,Tipo_Cobro_Mora
                            ,IncInclusiones,IncExclusiones,IncModificaciones,IncMantienen,TRANSITO_PLANILLAS_MES,TRANSITO_COMPARA, FRECUENCIA) 
		                values(@Descripcion, @DescCorta, @Activa, @Divisa, @MoraAutomatica, @DeducionPlanilla, 
                            @Direccion, @PlanillaRecibe, @PlanillaEnvio, @CtaCredito, @CtaObrero, @CtaPatronal, 
                            @CtaFondos, @CtaInconsistencias, @TipoAsiento, @PorcentajeAhorro, @PorcentajeAporte, 
                            @FechaCorte, @GeneraDeducciones, @CargaDeducciones, @Desgloza, @AhAplica, 
                            @AhDevoluciones, @AhInconsistencias, @CRAplica, @CRInconsistencias, @CRRecalculaMora,
                            @Inconsistencias, @Devoluciones, @DevOp, @DevPlan, @DevPlanPat, @FNDSocios, 
                            @OPSocios, @PlanSocios, @FNDExSocios, @OPExSocios, @PlanExSocios, 
                            @CodigoDeducAportes, @CodigoDeducCreditos, @CodigoEnvAportes, @CodigoEnvCreditos,
                            @CambiaFechaGeneral, @ChkCompara, @Compara, @CodInstDeduc, @HistoricoCuotasEnviadas,
                            @CuotasMora, @MovInclusion, @MovExclusion, @MovModificacion, @MovMantienen, 
                            @TransitoPlanillasMes, @TransitoCompra, @TipoPago)";

                        connection.Execute(query,
                            new
                            {
                                Descripcion = Info.descripcion.Trim(),
                                DescCorta = Info.desc_corta,
                                Activa = Info.activa ? 1 : 0,
                                Divisa = Info.cod_divisa,
                                MoraAutomatica = Info.mora_cierres ? 1 : 0,
                                DeducionPlanilla = Info.deduccion_planilla,
                                Direccion = Info.direccion,
                                PlanillaRecibe = Info.planilla,
                                PlanillaEnvio = Info.planilla_envio,
                                CtaCredito,
                                CtaObrero,
                                CtaPatronal,
                                CtaFondos,
                                CtaInconsistencias,
                                TipoAsiento = Info.tipoasiento,
                                PorcentajeAhorro = Info.porc_ahorro,
                                PorcentajeAporte = Info.porc_aporte,
                                FechaCorte = Info.pr_fecha_corte,
                                GeneraDeducciones = Info.pr_genera ? 1 : 0,
                                CargaDeducciones = Info.pr_carga ? 1 : 0,
                                Desgloza = Info.pr_desgloza ? 1 : 0,
                                AhAplica = Info.pr_apaplica ? 1 : 0,
                                AhInconsistencias = Info.pr_apinco ? 1 : 0,
                                AhDevoluciones = Info.pr_apdev ? 1 : 0,
                                CRAplica = Info.pr_craplica ? 1 : 0,
                                CRInconsistencias = Info.pr_crinco ? 1 : 0,
                                CRRecalculaMora = Info.pr_crmora ? 1 : 0,
                                Inconsistencias = Info.pr_cr_aplica_incon ? 1 : 0,
                                Devoluciones = Info.fnd_ap_aplica ? 1 : 0,
                                DevOp = Info.fnd_ap_operadora,
                                DevPlan = Info.fnd_ap_plan,
                                DevPlanPat = Info.fnd_ap_planp,
                                FNDSocios = Info.fnd_cr_soaplica ? 1 : 0,
                                OPSocios = Info.fnd_cr_sooperadora,
                                PlanSocios = Info.fnd_cr_soplan,
                                FNDExSocios = Info.fnd_cr_exaplica ? 1 : 0,
                                OPExSocios = Info.fnd_cr_exoperadora,
                                PlanExSocios = Info.fnd_cr_explan,
                                CodigoDeducAportes = Info.codigo_aportes,
                                CodigoDeducCreditos = Info.codigo_creditos,
                                CodigoEnvAportes = Info.codigo_aportes_env,
                                CodigoEnvCreditos = Info.codigo_creditos_env,
                                CambiaFechaGeneral = Info.ind_cambia_fecpro,
                                ChkCompara = Info.compara_indicador ? 1 : 0,
                                Compara = Info.compara_valor,
                                CodInstDeduc = Info.codigo_inst_deduc,
                                HistoricoCuotasEnviadas = Info.historico_cobro_envio,
                                CuotasMora = Info.tipo_cobro_mora,
                                MovInclusion = Info.incinclusiones ? 1 : 0,
                                MovExclusion = Info.incexclusiones ? 1 : 0,
                                MovModificacion = Info.incmodificaciones ? 1 : 0,
                                MovMantienen = Info.incmantienen ? 1 : 0,
                                TransitoPlanillasMes = Info.transito_planillas_mes,
                                TransitoCompra = Info.transito_compara,
                                TipoPago = Info.frecuencia_id
                            }
                        );

                        //Extraer el Ultimo
                        var queryU = "select isnull(max(cod_institucion),0) as Ultimo from instituciones";
                        int ultimo = connection.QueryFirstOrDefault<int>(queryU);

                        //Inserta Departamentos y Secciones por Omision
                        var queryD = "insert Afdepartamentos(cod_institucion,cod_departamento,descripcion) values(@Codigo,'','SIN IDENTIFICAR')";
                        connection.Execute(queryD, new { Codigo = ultimo });

                        var queryS = "insert AfSecciones(cod_institucion,cod_departamento,cod_seccion,descripcion) values(@Codigo,'','','SIN IDENTIFICAR')";
                        connection.Execute(queryS, new { Codigo = ultimo });

                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Institución No." + ultimo,
                            Movimiento = "Registra - WEB",
                            Modulo = 9
                        });

                        response.Code = ultimo;
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
        /// Eliminar institución
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodInstitucion"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_Institucion_Eliminar(int CodEmpresa, int CodInstitucion, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //var queryS = "delete AfSecciones where cod_institucion = @CodInstitucion";
                    //connection.Execute(queryS, new { CodInstitucion });

                    //var queryD = "delete Afdepartamentos where cod_institucion = @CodInstitucion";
                    //connection.Execute(queryD, new { CodInstitucion });

                    var query = "delete instituciones where cod_institucion = @CodInstitucion";
                    connection.Execute(query, new { CodInstitucion });

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Institución No." + CodInstitucion,
                        Movimiento = "Elimina - WEB",
                        Modulo = 9
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
    }
}
