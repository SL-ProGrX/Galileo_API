using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoReportesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MCobroDb _mCobroDb;
        private readonly int vModulo = 4;

        public FrmCoReportesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mCobroDb = new MCobroDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene el catálogo fijo de reportes de la pantalla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoReporteItemDto>> CO_Reportes_Catalogo_Obtener(int CodEmpresa)
        {
            var lista = new List<CoReporteItemDto>
            {
                new() { codigo = "01", descripcion = "Listado General" },
                new() { codigo = "02", descripcion = "Listado por Garantía" },
                new() { codigo = "03", descripcion = "Listado por Líneas" },
                new() { codigo = "04", descripcion = "Listado por Estado Persona" },
                new() { codigo = "05", descripcion = "Listado por Institución" },
                new() { codigo = "05.1", descripcion = "Listado por Deductora" },
                new() { codigo = "06", descripcion = "Listado por Comité Evaluador" },
                new() { codigo = "07", descripcion = "Listado por Provincia" },
                new() { codigo = "08", descripcion = "Antiguedad de Saldos" },
                new() { codigo = "08.1", descripcion = "Antiguedad de Saldos - Garantía" },
                new() { codigo = "08.2", descripcion = "Antiguedad de Saldos - Comité" },
                new() { codigo = "09", descripcion = "Antiguedad Mora Legal" },
                new() { codigo = "09.1", descripcion = "Antiguedad Mora Legal - Garantía" },
                new() { codigo = "09.2", descripcion = "Antiguedad Mora Legal - Comité" }
            };

            return DbHelper.CreateOkResponse(lista);
        }

        /// <summary>
        /// Obtiene líneas de crédito para F4/buscador.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Lineas_Obtener(int CodEmpresa, string? texto)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                texto = (texto ?? string.Empty).Trim();
                var like = texto.Length > 0 ? $"%{texto}%" : null;

                const string sql = @"
                    select
                        rtrim(codigo) as item,
                        rtrim(descripcion) as descripcion
                    from catalogo
                    where linea_interna = 1
                      and (@texto = '' or codigo like @like or descripcion like @like)
                    order by descripcion;";

                return conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    texto,
                    like
                }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la descripción de una línea/código usando MCobroDb.fxDescribeCodigo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CoReporteCodigoDescripcionDto> CO_Linea_Descripcion_Obtener(int CodEmpresa, string codigo)
        {
            try
            {
                codigo = (codigo ?? string.Empty).Trim();

                var descripcion = _mCobroDb.fxDescribeCodigo(CodEmpresa, codigo);

                return DbHelper.CreateOkResponse(new CoReporteCodigoDescripcionDto
                {
                    codigo = codigo,
                    descripcion = descripcion
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoReporteCodigoDescripcionDto>(ex.Message, -1, new CoReporteCodigoDescripcionDto());
            }
        }

        /// <summary>
        /// Obtiene lista de recursos. Si todasLineas = true trae todos; si no, filtra por código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="todasLineas"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Recursos_Dropdown_Obtener(int CodEmpresa, string? codigo, bool todasLineas)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                codigo = (codigo ?? string.Empty).Trim();

                const string sqlTodas = @"
                    select
                        rtrim(cod_grupo) as item,
                        rtrim(descripcion) as descripcion
                    from catalogo_grupos
                    order by descripcion;";

                const string sqlPorLinea = @"
                    select
                        rtrim(r.cod_grupo) as item,
                        rtrim(r.descripcion) as descripcion
                    from catalogo_grupos r
                    inner join catalogo_asignagrp a on r.cod_grupo = a.cod_grupo
                    where a.codigo = @codigo
                    order by r.descripcion;";

                if (todasLineas || codigo.Length == 0)
                {
                    return conn.Query<DropDownListaGenericaModel>(sqlTodas).ToList();
                }

                return conn.Query<DropDownListaGenericaModel>(sqlPorLinea, new { codigo }).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de destinos. Si todasLineas = true trae todos; si no, filtra por código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="todasLineas"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Destinos_Dropdown_Obtener(int CodEmpresa, string? codigo, bool todasLineas)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                codigo = (codigo ?? string.Empty).Trim();

                const string sqlTodas = @"
                    select
                        rtrim(cod_destino) as item,
                        rtrim(descripcion) as descripcion
                    from catalogo_destinos
                    order by descripcion;";

                const string sqlPorLinea = @"
                    select
                        rtrim(r.cod_destino) as item,
                        rtrim(r.descripcion) as descripcion
                    from catalogo_destinos r
                    inner join catalogo_destinosasg a on r.cod_destino = a.cod_destino
                    where a.codigo = @codigo
                    order by r.descripcion;";

                if (todasLineas || codigo.Length == 0)
                {
                    return conn.Query<DropDownListaGenericaModel>(sqlTodas).ToList();
                }

                return conn.Query<DropDownListaGenericaModel>(sqlPorLinea, new { codigo }).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de comités.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        cast(id_comite as varchar(20)) as item,
                        rtrim(descripcion) as descripcion
                    from comites
                    order by descripcion;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de instituciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        cast(cod_institucion as varchar(20)) as item,
                        rtrim(descripcion) as descripcion
                    from instituciones
                    order by descripcion;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de deductoras vinculadas por institución.
        /// Si no se especifica institución, trae todas las instituciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Deductoras_Dropdown_Obtener(int CodEmpresa, int? codInstitucion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (!codInstitucion.HasValue || codInstitucion.Value <= 0)
                {
                    const string sql = @"
                        select
                            cast(cod_institucion as varchar(20)) as item,
                            rtrim(descripcion) as descripcion
                        from instituciones
                        order by descripcion;";

                    return DbHelper.CreateOkResponse(
                        conn.Query<DropDownListaGenericaModel>(sql).ToList()
                    );
                }

                var lista = conn.Query<DropDownListaGenericaModel>(
                    "spAFI_Institucion_Vinculadas",
                    new
                    {
                        Institucion = codInstitucion.Value,
                        Tipo = 3
                    },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1, new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene lista de divisas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="gEnlace"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Divisas_Dropdown_Obtener(int CodEmpresa, int gEnlace)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(cod_divisa) as item,
                        rtrim(descripcion) as descripcion
                    from cntx_divisas
                    where cod_contabilidad = @gEnlace
                    order by descripcion;";

                return conn.Query<DropDownListaGenericaModel>(sql, new { gEnlace }).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de estados laborales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_EstadosLaborales_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(estado_laboral) as item,
                        rtrim(descripcion) as descripcion
                    from afi_estado_laboral
                    where activo = 1
                    order by descripcion asc;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de usuarios de gestión externa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Gestiona_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(usuario) as item,
                        rtrim(usuario) as descripcion
                    from cbr_usuarios
                    where operador_externo = 1
                    order by usuario;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de garantías.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(garantia) as item,
                        rtrim(descripcion) as descripcion
                    from crd_garantia_tipos
                    order by descripcion;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de antigüedades e incluye la opción manual CBJ.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Antiguedades_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        rtrim(cod_antiguedad) as item,
                        rtrim(descripcion) as descripcion
                    from cbr_antiguedad_tipos
                    order by cod_antiguedad;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql).ToList();

                lista.Add(new DropDownListaGenericaModel
                {
                    item = "CBJ",
                    descripcion = "9. Cbr.Judicial"
                });

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1, new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene lista de carteras.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_Carteras_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(cod_clasificacion) as item,
                        rtrim(descripcion) as descripcion
                    from cbr_clasificacion_cartera
                    where estado = 1
                    order by cod_clasificacion;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de estados de persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(cod_estado) as item,
                        rtrim(descripcion) as descripcion
                    from afi_estados_persona
                    where activo = 1
                    order by cod_estado;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Ejecuta el proceso de cubo de antigüedad/mora días reales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CO_Reportes_Cubo_Procesar(int CodEmpresa, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Execute(
                    "spCbrAntiguedadDiasRealAnalisisCubo",
                    commandType: CommandType.StoredProcedure);

                usuario = (usuario ?? string.Empty).Trim().ToUpperInvariant();

                var bitacora = Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = "Procesa cubo de antigüedad/mora días reales",
                    Movimiento = "Procesa - WEB",
                    Modulo = vModulo
                });

                if (bitacora.Code != 0)
                {
                    return bitacora;
                }

                return DbHelper.OkResponse("Proceso concluido correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}