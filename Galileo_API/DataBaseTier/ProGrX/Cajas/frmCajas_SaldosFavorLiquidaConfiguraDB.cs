using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.Security;
using Microsoft.Extensions.Configuration;

namespace Galileo.DataBaseTier
{

    public class FrmCajasSaldosFavorLiquidaConfiguraDB
    {
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 9;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCajasSaldosFavorLiquidaConfiguraDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
            _Security_MainDB = new MSecurityMainDb(config);
        }


        /// <summary>
        /// Consulta los tipo de saldos a favor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CajasSaldosFavorTiposLista> CajasSaldosFavorTipos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<CajasSaldosFavorTiposLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new CajasSaldosFavorTiposLista()
                {
                    total = 0,
                    lista = new List<CajasSaldosFavorTiposData>()
                }
            };

            try
            {
                filtros ??= new FiltrosLazyLoadData();
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                var totalQuery = "select COUNT(1) from CAJAS_SALDOS_FAVOR_TIPOS";
                result.Result.total = connection.Query<int>(totalQuery).FirstOrDefault();

                var sortField = string.IsNullOrWhiteSpace(filtros.sortField)
                    ? "DOC_TIPO"
                    : filtros.sortField;

                var query = @"select DOC_TIPO,descripcion,Activo from CAJAS_SALDOS_FAVOR_TIPOS ";

                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    query += @" WHERE ( DOC_TIPO LIKE '%'+@filtro+'%' OR descripcion LIKE '%'+@filtro+'%' ) ";
                }

                query += $@" order by {sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY";

                result.Result.lista = connection.Query<CajasSaldosFavorTiposData>(query, new
                {
                    filtro = filtros.filtro
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<CajasSaldosFavorTiposData>();
            }
            return result;
        }
       
        /// <summary>
       /// Consulta la configuracion de saldos a favor de un usuario
       /// </summary>
       /// <param name="CodEmpresa"></param>
       /// <param name="usuario"></param>
       /// <returns></returns>
        public ErrorDto<CajasSaldosFavorUsuarioLiquidaLista> CajasSaldosFavorUsuariosLiquida_Obtener(int CodEmpresa, string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var result = new ErrorDto<CajasSaldosFavorUsuarioLiquidaLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new CajasSaldosFavorUsuarioLiquidaLista()
                {
                    total = 0,
                    lista = new List<CajasSaldosFavorUsuarioLiquidData>()
                }
            };

            try
            {
                var query = $@"select T.DOC_TIPO,T.DESCRIPCION, isnull(L.ENVIA_FONDO,0) as envia_fondo, isnull(L.ENVIA_TESORERIA,0) as envia_tesoreria
                        , isnull(L.RET_EFECTIVO,0) as ret_efectivo, isnull(L.EXCLUYE_SALDO_FAVOR,0) as excluye_saldo_favor
                        from CAJAS_SALDOS_FAVOR_TIPOS T left join CAJAS_SALDOS_FAVOR_USUARIOS_LIQUIDA L
                        on T.DOC_TIPO = L.DOC_TIPO  and L.USUARIO = @usuario
                         Where T.ACTIVO = 1 ";


                result.Result.lista = connection.Query<CajasSaldosFavorUsuarioLiquidData>(query, new
                {
                    usuario
                }).ToList();

                result.Result.total = result.Result.lista.Count;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<CajasSaldosFavorUsuarioLiquidData>();
            }
            return result;
        }

        /// <summary>
        /// Inserta o actualiza el registros de un tipo de saldo a favor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto CajasSaldosFavorTipos_Guardar(int CodEmpresa, string usuario, CajasSaldosFavorTiposData data)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {

                var query = $@"select isnull(count(*),0) as Existe from CAJAS_SALDOS_FAVOR_TIPOS where DOC_TIPO =@doc_tipo";
                int existe = connection.QueryFirstOrDefault<int>(query, new { data.doc_tipo });

                if (existe > 0)
                {
                    result.Code = 1;
                    query = $@"update CAJAS_SALDOS_FAVOR_TIPOS set descripcion = @descripcion,Activo =@activo  where DOC_TIPO = @doc_tipo ";
                }
                else
                {

                    query = $@"insert CAJAS_SALDOS_FAVOR_TIPOS(DOC_TIPO,descripcion,Activo,Registro_Usuario,Registro_Fecha) 
                                values(@doc_tipo,@descripcion,@activo,@usuario,dbo.MyGetdate()) ";


                }

                connection.Query<int>(query, new
                {
                    data.doc_tipo,
                    data.descripcion,
                    data.activo,
                    usuario
                });

                if (result.Code == 1)
                {
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Saldo a Favor: {data.doc_tipo}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
                }
                else
                {
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Saldo a Favor: {data.doc_tipo}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });
                }


            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;

        }

        /// <summary>
        /// Elimina un tipo de saldo a favor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="doc_tipo"></param>
        /// <returns></returns>
        public ErrorDto CajasSaldosFavorTipos_Eliminar(int CodEmpresa, string usuario, string doc_tipo)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                var query = $@"delete CAJAS_SALDOS_FAVOR_TIPOS where DOC_TIPO =@doc_tipo";
                connection.Execute(query, new { doc_tipo });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Saldo a Favor: {doc_tipo}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Asigna la configuracion de saldos a favor de un usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuarioG"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto CajasSaldosFavorTipoLiq_Asigna(int CodEmpresa, string usuarioG, CajasSaldosFavorUsuarioLiquidData data)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {

                var query = $@"exec spCajas_SaldoFavorTipoLiqAsigna @doc_tipo,@envia_fondo,@envia_tesoreria,@ret_efectivo,@usuario,@usuarioG,@excluye_saldo_favor";


                connection.Query<int>(query, new
                {
                    data.doc_tipo,
                    envia_fondo = data.envia_fondo ? 1 : 0,
                    envia_tesoreria = data.envia_tesoreria ? 1 : 0,
                    ret_efectivo = data.ret_efectivo ? 1 : 0,
                    excluye_saldo_favor = data.excluye_saldo_favor ? 1 : 0, 
                    data.usuario,
                    usuarioG
                });
  
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;

        }

        /// <summary>
        /// Consulta el listado de usuarios posibles de configurar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CajasSaldosFavor_Usuarios_Obtener(int CodEmpresa)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                var query = $@"select Nombre as 'item',DESCRIPCION as 'descripcion'  
                                    from USUARIOS  
                                        where ESTADO = 'A'  and NOMBRE in(select USUARIO from CAJAS_USUARIOS group by USUARIO ) ";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<DropDownListaGenericaModel>();
            }
            return result;
        }


    }


}