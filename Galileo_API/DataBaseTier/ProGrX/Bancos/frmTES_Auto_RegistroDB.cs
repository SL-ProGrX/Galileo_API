using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using Microsoft.ReportingServices.Diagnostics.Internal;


namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesAutoRegistroDB
    {
        private readonly PortalDB _portalDB;

        public FrmTesAutoRegistroDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config!);
        }

        /// <summary>
        /// Método para consultar un registro de auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="autoReg"></param> 
        /// <returns></returns>
        public ErrorDto<TesAutoRegistroDto> Tes_AutoRegistro_Consultar(int CodEmpresa, int autoReg)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select * from vTES_AUTO_REGISTRO where ID_Auto = @autoReg";

                return conn.QueryFirstOrDefault<TesAutoRegistroDto>(query, new { autoReg }) ?? new TesAutoRegistroDto();
            });
        }

        /// <summary>
        /// Método para guardar un registro de auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="registro"></param>
        /// <returns></returns>
        public ErrorDto Tes_AutoRegistro_Guardar(int CodEmpresa, TesAutoRegistroDto registro)
        {
            try
            {

                int activo = (registro.id_auto == 0 || (registro.activo ?? false)) ? 1 : 0;

                string sql = @" EXEC spTes_Auto_Registro_Add
                                    @AutoId,
                                    @Descripcion,
                                    @PClave,
                                    @Detalle,
                                    @Concepto,
                                    @Cuenta,
                                    @Unidad,
                                    @CentroCosto,
                                    @MntInicio,
                                    @MntCorte,
                                    @Apl_CargaDiaria,
                                    @Apl_Conciliacion,
                                    @IndInfoPersona,
                                    @TipoBeneficiario,
                                    @BeneficiarioId,
                                    @BeneficiarioNombre,
                                    @Activo,
                                    @Usuario,
                                    @Mov,
                                    @TipoMov,
                                    @TipoDoc,
                                    @IgnoraRegistro,
                                    @FiltraCtas
                                ";

                var parametros = new
                {
                    AutoId = registro.id_auto,
                    Descripcion = registro.descripcion,
                    PClave = registro.palabras_clave,
                    Detalle = registro.detalle,
                    Concepto = registro.cod_concepto,
                    Cuenta = (registro.cod_cuenta_mask == null) ? "0" : registro.cod_cuenta_mask.Replace("-", ""),
                    Unidad = registro.cod_unidad,
                    CentroCosto = registro.cod_centro_costo,
                    MntInicio = (registro.mnt_inicio == null) ? 0 : registro.mnt_inicio,
                    MntCorte = (registro.mnt_corte == null) ? 0 : registro.mnt_corte,
                    Apl_CargaDiaria = (registro.apl_carga_diaria == true) ? 1 : 0,
                    Apl_Conciliacion = (registro.apl_conciliacion == true) ? 1 : 0,
                    IndInfoPersona = (registro.ind_info_persona == true) ? 1 : 0,
                    TipoBeneficiario = (registro.tipo_beneficiario == null) ? 0 : Convert.ToInt16(registro.tipo_beneficiario),
                    BeneficiarioId = registro.beneficiario_id,
                    BeneficiarioNombre = registro.beneficiario_nombre,
                    Activo = activo,
                    Usuario = (registro.id_auto! == 0) ? registro.registro_usuario! : registro.modifica_usuario!,
                    Mov = (registro.id_auto == 0) ? "A" : "M",
                    TipoMov = registro.apl_tipo_mov,
                    TipoDoc = registro.tipo_doc,
                    IgnoraRegistro = (registro.ignora_registro == true) ? 1 : 0,
                    FiltraCtas = (registro.filtra_cta_bancos == true) ? 1 : 0
                };

                var response = DbHelper.ExecuteSingleQuery <AutoRegGuardar>(_portalDB, CodEmpresa, sql, null, parametros);

                return DbHelper.OkResponse(response.Result!.auto_id.ToString());
 
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Método para eliminar un registro de auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="autoReg"></param>
        /// <returns></returns>
        public ErrorDto Tes_AutoRegistro_Eliminar(int CodEmpresa, TesAutoRegistroDto registro)
        {
            try
            {
                if(registro == null)
                {
                    return DbHelper.ErrorResponse("Debe especificar un registro para eliminar");
                }

                string sql = @" EXEC spTes_Auto_Registro_Add
                                    @AutoId,
                                    @Descripcion,
                                    @PClave,
                                    @Detalle,
                                    @Concepto,
                                    @Cuenta,
                                    @Unidad,
                                    @CentroCosto,
                                    @MntInicio,
                                    @MntCorte,
                                    @Apl_CargaDiaria,
                                    @Apl_Conciliacion,
                                    @IndInfoPersona,
                                    @TipoBeneficiario,
                                    @BeneficiarioId,
                                    @BeneficiarioNombre,
                                    @Activo,
                                    @Usuario,
                                    @Mov,
                                    @TipoMov,
                                    @TipoDoc,
                                    @IgnoraRegistro,
                                    @FiltraCtas
                                ";

                var parametros = new
                {
                    AutoId = registro.id_auto,
                    Descripcion = registro.descripcion,
                    PClave = registro.palabras_clave,
                    Detalle = registro.detalle,
                    Concepto = registro.cod_concepto,
                    Cuenta = registro.cod_cuenta,
                    Unidad = registro.cod_unidad,
                    CentroCosto = registro.cod_centro_costo,
                    MntInicio = (registro.mnt_inicio == null) ? 0 : registro.mnt_inicio,
                    MntCorte = (registro.mnt_corte == null) ? 0 : registro.mnt_corte,
                    Apl_CargaDiaria = (registro.apl_carga_diaria == true) ? 1 : 0,
                    Apl_Conciliacion = (registro.apl_conciliacion == true) ? 1 : 0,
                    IndInfoPersona = (registro.ind_info_persona == true) ? 1 : 0,
                    TipoBeneficiario = registro.tipo_beneficiario,
                    BeneficiarioId = registro.beneficiario_id,
                    BeneficiarioNombre = registro.beneficiario_nombre,
                    Activo = (registro.activo == true) ? 1 : 0,
                    Usuario = (registro.modifica_usuario != null) ? registro.modifica_usuario : registro.registro_usuario,
                    Mov = "E",
                    TipoMov = registro.apl_tipo_mov,
                    TipoDoc = registro.tipo_doc,
                    IgnoraRegistro = (registro.ignora_registro == true) ? 1 : 0,
                    FiltraCtas = (registro.filtra_cta_bancos == true) ? 1 : 0
                };

                var response = DbHelper.ExecuteSingleQuery<AutoRegGuardar>(_portalDB, CodEmpresa, sql, null, parametros);

                return DbHelper.OkResponse(response.Result!.auto_id.ToString());
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Método para obtener las cuentas bancarias asociadas a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="FiltraCtas"></param>
        /// <returns></returns>
        public ErrorDto<List<TesAutoRegCtaBancariasData>> Tes_AutoRegistroCtaBancos_Obtener(int CodEmpresa, int? codigo, string? FiltraCtas)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"exec spTes_Auto_Registro_Ctas @AutoId , @Descripcion";
                string? valCodigo = (codigo == null || codigo == 0) ? "0" : codigo.ToString();

                return conn.Query<TesAutoRegCtaBancariasData>(query, new { AutoId = valCodigo, Descripcion = FiltraCtas }).ToList();
            });
        }

        /// <summary>
        /// Método para asignar o des asignar una cuenta bancaria a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CtaBanco"></param>
        /// <returns></returns>
        public ErrorDto Tes_AutoRegistroCtaBancos_Asignar(int CodEmpresa, int codigo , int cta ,bool asignado, string usuario )
        {
            var sql = @"exec spTes_Auto_Registro_Ctas_Add @AutoId, @BancoId, @Mov, @Usuario";
            var parametros = new
            {
                AutoId = codigo,
                BancoId = cta,
                Mov = asignado ? "A" : "E",
                Usuario = usuario
            };
            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, parametros);
        }


        /// <summary>
        /// Método para obtener los tipos de auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroTipos_Obtener(int CodEmpresa, int? tipo = 0, string? filtro = null)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                // Si filtro viene null, lo tratamos como vacío
                filtro ??= "";
                if(tipo == null)
                {
                    tipo = 0;
                }

                string sql = tipo switch
                {
                    1 => @"Select Cedula as item, Nombre as descripcion
                   from Socios
                   where Cedula like @filtro",

                    2 => @"select ID_BANCO as item, descripcion as descripcion
                   from TES_BANCOS
                   where ID_BANCO like @filtro",

                    3 => @"Select CEDJUR as item, DESCRIPCION as descripcion
                   from CXP_PROVEEDORES
                   where CEDJUR like @filtro",

                    4 => @"select Cod_Acreedor as item, DESCRIPCION as descripcion
                   from CRD_APA_ACREEDORES
                   where cod_acreedor like @filtro",

                    5 => @"Select IDENTIFICACION as item, NOMBRE_COMPLETO as descripcion
                   from RH_PERSONAS
                   where IDENTIFICACION like @filtro",

                    6 => @"Select CODIGO as item, BENEFICIARIO as descripcion
                   from vTes_Beneficiarios
                   where CODIGO like @filtro",

                    _ => @"Select '' as item, '' as descripcion Where 1 = 0" // Consulta vacía para tipos no reconocidos
                };

                // Ojo: el comodín % va en el valor del parámetro
                var param = new { filtro = $"%{filtro}%" };

                return conn.Query<DropDownListaGenericaModel>(sql, param).ToList();
            });
        }

        /// <summary>
        /// Método para obtener los centros de costos asociados a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroCentroCostos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select COD_CENTRO_COSTO as 'item', DESCRIPCION from vCNTX_CENTRO_COSTO_LOCAL";
                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Método para obtener los códigos y descripciones de conceptos, unidades o centros de costos asociados a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroCodigoDesc_Obtener(int CodEmpresa, string tipo, string codigo)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var query = "";
                switch (tipo)
                {
                    case "Cta":
                    case "Con":
                        query = $@"select cod_concepto as 'item', DESCRIPCION as 'descripcion' , from vTes_Conceptos Where cod_concepto = @codigo";
                        break;
                    case "Ud":
                        query = $@"select cod_Unidad as 'item', DESCRIPCION as 'descripcion' from vCNTX_UNIDADES_LOCAL Where cod_Unidad = @codigo";
                        break;
                    case "Cc":
                        query = $@"select cod_Centro_Costo as 'item', DESCRIPCION as 'descripcion' from vCNTX_CENTRO_COSTO_LOCAL Where cod_Centro_Costo = @codigo";
                        break;
                }
                return conn.Query<DropDownListaGenericaModel>(query, new { codigo  = codigo}).ToList();
            });
        }

        /// <summary>
        /// Método para obtener los conceptos de auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TesAutoregistroConceptos>> Tes_AutoRegistroConceptos_Obtener(int CodEmpresa, string? concepto = null)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var query = $"select COD_CONCEPTO, DESCRIPCION, COD_CUENTA_MASK, DP_TRAMITE_APL, CUENTA_DESC from vTes_Conceptos WHERE AUTO_REGISTRO = 1 AND ESTADO = 'A' and (@concepto is null or COD_CONCEPTO = @concepto) ";
                return conn.Query<TesAutoregistroConceptos>(query, new { concepto }).ToList();
            });
        }

        /// <summary>
        /// Método para obtener las unidades asociadas a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroCentroUnidades_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                var query = $@"select COD_UNIDAD as 'item', DESCRIPCION from vCNTX_UNIDADES_LOCAL";
                response.Result = conn.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Método para obtener los tipos de documentos asociados a un auto registro de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="TipoMov"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AutoRegistroTiposDoc_Obtener(int CodEmpresa, string TipoMov)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                var query = $@"exec spTes_Tipos_Docs @TipoMov ";
                var datos = conn.Query<TipoMovData>(query, new { TipoMov = TipoMov }).ToList();
                foreach (var item in datos)
                {
                    string idx = item.tipo!;
                    string itmx = item.descripcion!;

                    response.Result.Add(new DropDownListaGenericaModel { item = idx, descripcion = itmx });
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
        /// Método para obtener una lista de registros de auto registro de tesorería con paginación y filtros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesAutoRegistroLista> Tes_AutoRegistroLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var result = new ErrorDto<TesAutoRegistroLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesAutoRegistroLista
                {
                    total = 0,
                    lista = new List<TesAutoRegistroDto>()
                }
            };

            try
            {
                // Normaliza inputs
                var filtro = string.IsNullOrWhiteSpace(filtros?.filtro) ? null : filtros.filtro.Trim();

                // Importante: OFFSET y FETCH deben ser int (y con límites razonables)
                int offset = Math.Max(0, filtros?.pagina ?? 0);
                int fetch = Math.Clamp(filtros?.paginacion ?? 10, 1, 200); // ajusta el max a tu gusto

                // Para LIKE: el % va en el parámetro, NO en el SQL
                var like = filtro is null ? null : $"%{filtro}%";

                // Total con el mismo filtro (si quieres total “filtrado”)
                const string sqlCount = @"
                        select Count(*)
                        from vTES_AUTO_REGISTRO
                        where (@like is null
                               or id_auto like @like
                               or descripcion like @like
                               or palabras_clave like @like);";

                result.Result.total = conn.QuerySingle<int>(sqlCount, new { like });

                // Datos paginados
                const string sqlData = @"
                                select *
                                from vTES_AUTO_REGISTRO
                                where (@like is null
                                       or id_auto like @like
                                       or descripcion like @like
                                       or palabras_clave like @like)
                                order by id_auto
                                offset @offset rows fetch next @fetch rows only;";

                result.Result.lista = conn.Query<TesAutoRegistroDto>(sqlData, new { like, offset, fetch }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<TesAutoRegistroDto>();
            }

            return result;
        }

        /// <summary>
        /// Método para obtener un registro de auto registro de tesorería por ID mediente scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="autoReg"></param>
        /// <returns></returns>
        public ErrorDto<TesAutoRegistroDto> Tes_AutoRegistro_scroll(int CodEmpresa, int autoReg = 0, int? scroll = 0)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

           
            var response = new ErrorDto<TesAutoRegistroDto>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new TesAutoRegistroDto()
            };
            try
            {
                string where = "";
                scroll = (scroll == null) ? 0 : scroll;
                if (scroll == 1) //busca el registro anterior
                {
                    if (autoReg == 0 )
                    {
                        autoReg = 99999999;
                    }

                    where = $" WHERE ID_Auto < {autoReg} ORDER BY ID_Auto DESC ";
                }
                else if (scroll == 2) //busca el registro siguiente
                {
                    if (autoReg == 0 )
                    {
                        autoReg = 0;
                    }

                    where = $" WHERE ID_Auto > {autoReg} ORDER BY ID_Auto ASC ";
                }

                var query = $@"select top 1 * from vTES_AUTO_REGISTRO  ";
                query += where; 
                response.Result = conn.QueryFirstOrDefault<TesAutoRegistroDto>(query);
            }
            catch (Exception ex)
            {
                DbHelper.CreateErrorResponse<TesAutoRegistroDto>(ex.Message);
            }
            return response;

        }

    }
}
