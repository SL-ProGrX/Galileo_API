using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneficiosGruposDB
    {
        private readonly IConfiguration _config;
        private readonly mBeneficiosDB _mBeneficiosDB;

        public frmAF_BeneficiosGruposDB(IConfiguration config)
        {
            _config = config;
            _mBeneficiosDB = new mBeneficiosDB(_config);
        }

        public ErrorDto<AfiBeneGruposLista> AfiBeneGrupos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<AfiBeneGruposLista>();
            response.Result = new AfiBeneGruposLista();
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) from AFI_BENE_GRUPOS ";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " where cod_grupo LIKE '%" + filtro + "%' " +
                            "OR descripcion LIKE '%" + filtro + "%' " +
                            "OR Cod_Categoria like '%" + filtro + "%'";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"select cod_grupo,descripcion, Cod_Categoria ,monto,estado, User_Registra, Fecha from AFI_BENE_GRUPOS
                                         {filtro} 
                                        order by cod_grupo
                                        {paginaActual}
                                        {paginacionActual} ";

                    response.Result.beneficios = connection.Query<AfiBeneGrupos>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.beneficios = null;
            }
            return response;
        }

        private ErrorDto AfiBeneGrupos_Insertar(int CodCliente, AfiBeneGrupos grupo)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();

            try
            {

                int activo = grupo.estado ? 1 : 0;

                using var connection = new SqlConnection(stringConn);
                {
                    //Obtengo consecutivo
                    var query = $@"select isnull(max(cod_grupo),0) + 1 as consec  from afi_bene_grupos";
                    var consecutivo = connection.Query<int>(query).FirstOrDefault();

                    query = $@"INSERT INTO AFI_BENE_GRUPOS (cod_grupo,descripcion,cod_categoria,monto,estado,fecha,user_registra) 
                                VALUES ({consecutivo},'{grupo.descripcion}', '{grupo.cod_categoria}' ,{grupo.monto},{activo},getdate(),'{grupo.user_registra}')";
                    info.Code = connection.Execute(query);

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
                    {
                        EmpresaId = CodCliente,
                        cod_beneficio = grupo.cod_grupo.ToString(),
                        consec = -2,
                        movimiento = "Inserta frmAF_BeneficiosGrupos-Web",
                        detalle = $@"Inserta el monto del Beneficio, Codigo grupo: {grupo.cod_grupo}, {grupo.descripcion} por [{grupo.monto}]",
                        registro_usuario = grupo.user_registra
                    });
                }

                info.Description = "Registro Insertado";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;

        }

        private ErrorDto AfiBeneGrupos_Actualizar(int CodCliente, AfiBeneGrupos grupo)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            try
            {
                int estado = grupo.estado ? 1 : 0;
                using var connection = new SqlConnection(stringConn);
                {

                    var query1 = $@"SELECT MONTO FROM AFI_BENE_GRUPOS 
                            WHERE COD_CATEGORIA = '{grupo.cod_categoria}' AND COD_GRUPO = {grupo.cod_grupo}";
                    int MONTO_ANTERIOR = connection.Query<int>(query1).FirstOrDefault();


                    var query = $@"UPDATE AFI_BENE_GRUPOS SET descripcion = '{grupo.descripcion}', cod_categoria = '{grupo.cod_categoria}' ,monto = {grupo.monto},estado = {estado}  
                                WHERE cod_grupo = {grupo.cod_grupo}";
                    info.Code = connection.Execute(query);


                    if (grupo.monto != MONTO_ANTERIOR)
                    {

                        _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
                        {
                            EmpresaId = CodCliente,
                            cod_beneficio = grupo.cod_grupo.ToString(),
                            consec = -2,
                            movimiento = "Actualiza",
                            detalle = $@"Actualiza el monto del Beneficio, Codigo grupo: {grupo.cod_grupo}, {grupo.descripcion} de [{MONTO_ANTERIOR}] por [{grupo.monto}]",
                            registro_usuario = grupo.user_registra
                        });
                    }


                }

                info.Description = "Registro Actualizado";

                
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;

        }

        public ErrorDto AfiBeneGrupos_Eliminar(int CodCliente, int cod_grupo)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"DELETE FROM AFI_BENE_GRUPOS WHERE cod_grupo = {cod_grupo}";
                    connection.Execute(query);

                }

                info.Description = "Registro Eliminado";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;

        }

        public ErrorDto<AfiBeneGruposAsigandosLista> BeneficioUsuariosLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string cod_grupo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<AfiBeneGruposAsigandosLista>();
            response.Result = new AfiBeneGruposAsigandosLista();
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = $@"select count(*)
                                from  afi_beneficios B left join afi_Grupo_Beneficio G
                                on B.Cod_beneficio = G.cod_beneficio 
		                        and G.cod_grupo = '{cod_grupo}'";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " where B.cod_beneficio LIKE '%" + filtro + "%' OR B.descripcion LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"select B.cod_beneficio,B.descripcion,isnull(G.Cod_grupo,-1) as 'Marca',
                                    case when G.Cod_grupo is null then 0 else 1 end as 'activo'
                                from  afi_beneficios B left join afi_Grupo_Beneficio G
                                on B.Cod_beneficio = G.cod_beneficio 
                                       and G.cod_grupo = '{cod_grupo}'  
                                         {filtro} 
                                       order by G.cod_beneficio desc,B.descripcion 
                                        {paginaActual}
                                        {paginacionActual} ";

                    response.Result.beneficios = connection.Query<AfiBeneGruposAsigandosData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
               response.Code = -1;
                response.Description = ex.Message;
                response.Result.beneficios = null;
            }

            return response;

        }

        public ErrorDto AfiGrupoBeneficio_Insertar(int CodCliente, AfiGrupoBeneficioData grupo)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"insert AFI_GRUPO_BENEFICIO(cod_beneficio,cod_grupo) values ('{grupo.cod_beneficio}', {grupo.cod_grupo})";
                    connection.Execute(query);

                }

                info.Description = "Registro Insertado";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;

        }

        public ErrorDto AfiGrupoBeneficio_Eliminar(int CodCliente, AfiGrupoBeneficioData grupo)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"DELETE FROM AFI_GRUPO_BENEFICIO WHERE cod_beneficio = '{grupo.cod_beneficio}' and cod_grupo = {grupo.cod_grupo}";
                    connection.Execute(query);

                }

                info.Description = "Registro Eliminado";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;

        }

        public ErrorDto<List<AfiBeneGrupos>> AfiBeneGrupos_lista(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneGrupos>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select cod_grupo,descripcion,monto,estado from AFI_BENE_GRUPOS";
                    response.Result = connection.Query<AfiBeneGrupos>(query).ToList();

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

        public ErrorDto<List<AfiBeneLista>> AfiBeneCategoriaLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneLista>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select rtrim(COD_CATEGORIA) as 'item', rtrim(COD_CATEGORIA) + ' ' +  rtrim(DESCRIPCION) as 'descripcion' From AFI_BENE_CATEGORIAS
                                    Where Activo = 1 ORDER BY COD_CATEGORIA";
                    response.Result = connection.Query<AfiBeneLista>(query).ToList();

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

        public ErrorDto<List<AfiBeneAsignacionesData>> AfiAsignaciones_Obtener(int CodCliente, int asigna, string grupo)
        {
            switch (asigna)
            {
                case 0:
                    return spAsignaGrupo(CodCliente, grupo, "spAFI_Bene_Grupos_Estados_List");
                case 1:
                    return spAsignaGrupo(CodCliente, grupo, "spAFI_Bene_Grupos_Requisitos_List");
                case 2:
                    return spAsignaGrupo(CodCliente, grupo, "spAFI_Bene_Grupos_Motivos_List");
                case 3:
                    return spAsignaGrupo(CodCliente, grupo, "spAFI_Bene_Grupos_Accesos_List");
                default:
                    return new ErrorDto<List<AfiBeneAsignacionesData>>();
            }
        }


        private ErrorDto<List<AfiBeneAsignacionesData>> spAsignaGrupo(int CodCliente,string grupo, string storeProcedure )
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneAsignacionesData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = $"[{storeProcedure}]";
                    var values = new
                    {
                        GrupoId = grupo,
                    };

                    response.Result = connection.Query<AfiBeneAsignacionesData>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
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

        public ErrorDto AfiAsignaciones_Actualizar(int CodCliente,
            int asigna,
            string grupo,
            string valor,
            string usuario,
            string mov)
        {
            switch (asigna)
            {
                case 0:
                    return spAsigGrupoAdd(CodCliente, grupo, valor, "Estado", usuario, mov, "spAFI_Bene_Grupos_Estados_Add");
                case 1:
                    return spAsigGrupoAdd(CodCliente, grupo, valor, "Requisito", usuario, mov, "spAFI_Bene_Grupos_Requisitos_Add");
                case 2:
                    return spAsigGrupoAdd(CodCliente, grupo, valor, "Motivo", usuario, mov, "spAFI_Bene_Grupos_Motivos_Add");
                case 3:
                    return spAsigGrupoAdd(CodCliente, grupo, valor, "Rol", usuario, mov, "spAFI_Bene_Grupos_Accesos_Add");
                default:
                    return new ErrorDto();
            }
        }

        private ErrorDto spAsigGrupoAdd2(int CodCliente, string grupo, string valor, string parametro, string usuario, string mov, string storeProcedure)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            object values = null;  // Initialize as object to handle anonymous type assignment

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = $"[{storeProcedure}]";
                    switch (parametro)
                    {
                        case "Estado":
                            values = new
                            {
                                GrupoId = grupo,
                                Estado = valor,
                                Usuario = usuario,
                                Mov = mov
                            };
                            break;
                        case "Requisito":
                            values = new
                            {
                                GrupoId = grupo,
                                Requisito = valor,
                                Usuario = usuario,
                                Mov = mov
                            };
                            break;
                        case "Motivo":
                            values = new
                            {
                                GrupoId = grupo,
                                Motivo = valor,
                                Usuario = usuario,
                                Mov = mov
                            };
                            break;
                        case "Rol":
                            values = new
                            {
                                GrupoId = grupo,
                                Rol = valor,
                                Usuario = usuario,
                                Mov = mov
                            };
                            break;

                        default:
                            throw new ArgumentException("Invalid parameter value.", nameof(parametro));
                    }


                    resp.Code = connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        private ErrorDto spAsigGrupoAdd(int CodCliente, string grupo, string valor, string parametro, string usuario, string mov, string storeProcedure)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var resp = new ErrorDto { Code = 0 };
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);

                // Create a dictionary to map parameters to their field names
                var fields = new Dictionary<string, string>
                {
                    { "Estado", "Estado" },
                    { "Requisito", "Requisito" },
                    { "Motivo", "Motivo" },
                    { "Rol", "Rol" }
                };

                if (!fields.TryGetValue(parametro, out var fieldName))
                {
                    throw new ArgumentException("Invalid parameter value.", nameof(parametro));
                }

                // Create the anonymous object dynamically
                var values = new
                {
                    GrupoId = grupo,
                    FieldValue = valor,
                    Usuario = usuario,
                    Mov = mov
                };

                // Map the field name to the correct property in the object
                var valuesDictionary = new Dictionary<string, object>
                {
                    { "GrupoId", grupo },
                    { fieldName, valor },
                    { "Usuario", usuario },
                    { "Mov", mov }
                };

                // Execute the stored procedure with Dapper
                 connection.Execute($"[{storeProcedure}]", valuesDictionary, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                resp.Code = -1; 
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto AfiBeneGrupo_Guardar(int CodCliente, AfiBeneGrupos grupo)
        {
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            if (grupo.cod_grupo == 0)
            {
                info = AfiBeneGrupos_Insertar(CodCliente, grupo);
            }
            else
            {
                info = AfiBeneGrupos_Actualizar(CodCliente, grupo);
            }

            return info;
        }

        public ErrorDto<List<AfiBeneGrupos>> AfiBeneGrupoExportar(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneGrupos>>(); 
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cod_grupo,descripcion, Cod_Categoria ,monto,estado, User_Registra, Fecha from AFI_BENE_GRUPOS
                                        order by cod_grupo ";
                    response.Result = connection.Query<AfiBeneGrupos>(query).ToList();
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