using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoReportesTransitoDB
    {
        private readonly PortalDB _portalDB;
        private readonly MCobroDb _mCobroDb;

        public FrmCoReportesTransitoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mCobroDb = new MCobroDb(config);
        }

        /// <summary>
        /// Obtiene el catálogo de informes del formulario de morosidad real/tránsito.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa consultada.</param>
        /// <returns>Catálogo de informes del formulario original.</returns>
        public ErrorDto<List<CoReporteItemDto>> CO_ReportesTransito_Catalogo_Obtener(int CodEmpresa)
        {
            var lista = new List<CoReporteItemDto>
            {
                new() { codigo = "GENDET", descripcion = "Listado General" },
                new() { codigo = "GENTRA", descripcion = "Listado General + Cuota en Tránsito" },
                new() { codigo = "GENAGD", descripcion = "Listado General Agrupado" },
                new() { codigo = "GENIDR", descripcion = "Listado por Institución/Departamento" },
                new() { codigo = "GENIDL", descripcion = "Listado por Institución/Departamento/Línea" },
                new() { codigo = "ESPCON", descripcion = "Especial Convenios" },
                new() { codigo = "MORCAR", descripcion = "Comparativo - Resumen" },
                new() { codigo = "MORGAR", descripcion = "Mora por Garantía" },
                new() { codigo = "MORGAG", descripcion = "Mora por Garantía Agrupado" },
                new() { codigo = "ANTSALDOS", descripcion = "Antigüedad de Saldos" },
                new() { codigo = "ANTSALDOSPA", descripcion = "Antigüedad de Saldos + Producto Acumulado" },
                new() { codigo = "ANTPROACUM", descripcion = "Antigüedad de Producto Acumulado" },
                new() { codigo = "ANTLEGAL", descripcion = "Antigüedad de Saldos (Legal)" },
                new() { codigo = "ANTFINAN", descripcion = "Antigüedad de Saldos (Financiera)" },
                new() { codigo = "DETPRV", descripcion = "Listado por Provincia" },
                new() { codigo = "DETPVT", descripcion = "Listado por Provincia Trabajo" },
                new() { codigo = "DETUND", descripcion = "Listado por Unidad" },
                new() { codigo = "LSTFRM", descripcion = "Listado por Formalización" },
                new() { codigo = "LSTCMT", descripcion = "Listado por Comité Resolutor" }
            };

            return DbHelper.CreateOkResponse(lista);
        }

        /// <summary>
        /// Obtiene líneas de crédito para la búsqueda F4.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa consultada.</param>
        /// <param name="texto">Código o descripción utilizados como filtro.</param>
        /// <returns>Líneas de crédito coincidentes.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Lineas_Obtener(int CodEmpresa, string? texto)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                texto = (texto ?? string.Empty).Trim();
                var like = $"%{texto}%";
                const string sql = @"
                    select rtrim(codigo) as item, rtrim(descripcion) as descripcion
                    from catalogo
                    where linea_interna = 1
                      and (@texto = '' or codigo like @like or descripcion like @like)
                    order by descripcion;";

                return conn.Query<DropDownListaGenericaModel>(sql, new { texto, like }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la descripción de una línea de crédito.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa consultada.</param>
        /// <param name="codigo">Código de la línea.</param>
        /// <returns>Código y descripción de la línea.</returns>
        public ErrorDto<CoReporteCodigoDescripcionDto> CO_ReportesTransito_LineaDescripcion_Obtener(int CodEmpresa, string codigo)
        {
            try
            {
                codigo = (codigo ?? string.Empty).Trim();
                return DbHelper.CreateOkResponse(new CoReporteCodigoDescripcionDto
                {
                    codigo = codigo,
                    descripcion = _mCobroDb.fxDescribeCodigo(CodEmpresa, codigo)
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoReporteCodigoDescripcionDto>(
                    ex.Message,
                    -1,
                    new CoReporteCodigoDescripcionDto());
            }
        }

        /// <summary>
        /// Obtiene los recursos asociados a una línea o todos los recursos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa consultada.</param>
        /// <param name="codigo">Código de la línea.</param>
        /// <param name="todasLineas">Indica si se deben consultar todas las líneas.</param>
        /// <returns>Listado de recursos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Recursos_Obtener(int CodEmpresa, string? codigo, bool todasLineas)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                codigo = (codigo ?? string.Empty).Trim();
                const string sqlTodas = @"
                    select rtrim(cod_grupo) as item, rtrim(descripcion) as descripcion
                    from catalogo_grupos order by descripcion;";
                const string sqlLinea = @"
                    select rtrim(r.cod_grupo) as item, rtrim(r.descripcion) as descripcion
                    from catalogo_grupos r
                    inner join catalogo_asignagrp a on r.cod_grupo = a.cod_grupo
                    where a.codigo = @codigo order by r.descripcion;";

                return (todasLineas || codigo.Length == 0)
                    ? conn.Query<DropDownListaGenericaModel>(sqlTodas).ToList()
                    : conn.Query<DropDownListaGenericaModel>(sqlLinea, new { codigo }).ToList();
            });
        }

        /// <summary>
        /// Obtiene los destinos asociados a una línea o todos los destinos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa consultada.</param>
        /// <param name="codigo">Código de la línea.</param>
        /// <param name="todasLineas">Indica si se deben consultar todas las líneas.</param>
        /// <returns>Listado de destinos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Destinos_Obtener(int CodEmpresa, string? codigo, bool todasLineas)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                codigo = (codigo ?? string.Empty).Trim();
                const string sqlTodas = @"
                    select rtrim(cod_destino) as item, rtrim(descripcion) as descripcion
                    from catalogo_destinos order by descripcion;";
                const string sqlLinea = @"
                    select rtrim(r.cod_destino) as item, rtrim(r.descripcion) as descripcion
                    from catalogo_destinos r
                    inner join catalogo_destinosasg a on r.cod_destino = a.cod_destino
                    where a.codigo = @codigo order by r.descripcion;";

                return (todasLineas || codigo.Length == 0)
                    ? conn.Query<DropDownListaGenericaModel>(sqlTodas).ToList()
                    : conn.Query<DropDownListaGenericaModel>(sqlLinea, new { codigo }).ToList();
            });
        }

        /// <summary>
        /// Obtiene las instituciones disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa consultada.</param>
        /// <returns>Listado de instituciones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Instituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
                conn.Query<DropDownListaGenericaModel>(@"
                    select cast(cod_institucion as varchar(20)) as item,
                           rtrim(descripcion) as descripcion
                    from instituciones order by descripcion;").ToList());
        }

        /// <summary>
        /// Obtiene las deductoras vinculadas a una institución.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa consultada.</param>
        /// <param name="codInstitucion">Institución seleccionada; null consulta todas.</param>
        /// <returns>Listado de deductoras.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Deductoras_Obtener(int CodEmpresa, int? codInstitucion)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                if (!codInstitucion.HasValue || codInstitucion.Value <= 0)
                {
                    return conn.Query<DropDownListaGenericaModel>(@"
                        select cast(cod_institucion as varchar(20)) as item,
                               rtrim(descripcion) as descripcion
                        from instituciones order by descripcion;").ToList();
                }

                return conn.Query<DropDownListaGenericaModel>(
                    "spAFI_Institucion_Vinculadas",
                    new { Institucion = codInstitucion.Value, Tipo = 3 },
                    commandType: CommandType.StoredProcedure).ToList();
            });
        }

        /// <summary>
        /// Obtiene las garantías disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa consultada.</param>
        /// <returns>Listado de garantías.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Garantias_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
                conn.Query<DropDownListaGenericaModel>(@"
                    select rtrim(garantia) as item, rtrim(descripcion) as descripcion
                    from crd_garantia_tipos order by descripcion;").ToList());
        }

        /// <summary>
        /// Obtiene las oficinas disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa consultada.</param>
        /// <returns>Listado de oficinas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Oficinas_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
                conn.Query<DropDownListaGenericaModel>(@"
                    select rtrim(cod_oficina) as item, rtrim(descripcion) as descripcion
                    from sif_oficinas order by descripcion;").ToList());
        }

        /// <summary>
        /// Obtiene los estados de persona disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa consultada.</param>
        /// <returns>Listado de estados.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Estados_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
                conn.Query<DropDownListaGenericaModel>(@"
                    select rtrim(cod_estado) as item, rtrim(descripcion) as descripcion
                    from afi_estados_persona where activo = 1 order by cod_estado;").ToList());
        }

        /// <summary>
        /// Obtiene las clasificaciones de cartera disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa consultada.</param>
        /// <returns>Listado de carteras.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ReportesTransito_Carteras_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
                conn.Query<DropDownListaGenericaModel>(@"
                    select rtrim(cod_clasificacion) as item, rtrim(descripcion) as descripcion
                    from cbr_clasificacion_cartera
                    where estado = 1 order by cod_clasificacion;").ToList());
        }
    }
}
