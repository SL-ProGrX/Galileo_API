using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        /// <summary>
        /// Busca socios por identificación o nombre para la recepción de trámites.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionSocioItem>>
            Cr_SeguimientoTramites_Recepcion_Socios_Buscar(
                int codEmpresa,
                string? filtro)
        {
            const string sql = """
                select top 100
                    rtrim(cedula) as cedula,
                    rtrim(nombre) as nombre,
                    rtrim(isnull(EstadoActual, '')) as estado_actual
                from socios
                where cedula like '%' + @Filtro + '%'
                   or nombre like '%' + @Filtro + '%'
                order by nombre;
                """;

            return DbHelper.ExecuteListQuery<CrSeguimientoTramitesRecepcionSocioItem>(
                _portalDb,
                codEmpresa,
                sql,
                new { Filtro = Cr_SeguimientoTramites_Filtro_Normalizar(filtro, 100) });
        }

        /// <summary>
        /// Busca líneas de crédito activas y sin retención para la recepción de trámites.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionLineaItem>>
            Cr_SeguimientoTramites_Recepcion_Lineas_Buscar(
                int codEmpresa,
                string? filtro)
        {
            const string sql = """
                select top 100
                    rtrim(codigo) as codigo,
                    rtrim(descripcion) as descripcion,
                    rtrim(isnull(moneda, '')) as cod_divisa,
                    rtrim(isnull(base_calculo, '')) as base_calculo
                from catalogo
                where activo = 1
                  and retencion = 'N'
                  and (
                      codigo like '%' + @Filtro + '%'
                      or descripcion like '%' + @Filtro + '%'
                  )
                order by descripcion;
                """;

            return DbHelper.ExecuteListQuery<CrSeguimientoTramitesRecepcionLineaItem>(
                _portalDb,
                codEmpresa,
                sql,
                new { Filtro = Cr_SeguimientoTramites_Filtro_Normalizar(filtro, 100) });
        }

        /// <summary>
        /// Busca promotores por identificación o nombre para la recepción de trámites.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionPromotorItem>>
            Cr_SeguimientoTramites_Recepcion_Promotores_Buscar(
                int codEmpresa,
                string? filtro)
        {
            const string sql = """
                select top 100
                    ID_PROMOTOR as id_promotor,
                    rtrim(Nombre) as nombre
                from promotores
                where convert(varchar(20), ID_PROMOTOR) like '%' + @Filtro + '%'
                   or Nombre like '%' + @Filtro + '%'
                order by Nombre;
                """;

            return DbHelper.ExecuteListQuery<CrSeguimientoTramitesRecepcionPromotorItem>(
                _portalDb,
                codEmpresa,
                sql,
                new { Filtro = Cr_SeguimientoTramites_Filtro_Normalizar(filtro, 100) });
        }

        /// <summary>
        /// Busca proveedores por código, identificación jurídica o descripción.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionProveedorItem>>
            Cr_SeguimientoTramites_Recepcion_Proveedores_Buscar(
                int codEmpresa,
                string? filtro)
        {
            const string sql = """
                select top 100
                    cod_proveedor,
                    rtrim(isnull(cedjur, '')) as cedjur,
                    rtrim(descripcion) as descripcion
                from cxp_proveedores
                where convert(varchar(20), cod_proveedor) like '%' + @Filtro + '%'
                   or cedjur like '%' + @Filtro + '%'
                   or descripcion like '%' + @Filtro + '%'
                order by descripcion;
                """;

            return DbHelper.ExecuteListQuery<CrSeguimientoTramitesRecepcionProveedorItem>(
                _portalDb,
                codEmpresa,
                sql,
                new { Filtro = Cr_SeguimientoTramites_Filtro_Normalizar(filtro, 100) });
        }

        /// <summary>
        /// Obtiene persona, línea y catálogos dependientes para una recepción.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionLineaContextoData>
            Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionLineaContextoRequest request)
        {
            string usuario = Cr_SeguimientoTramites_Filtro_Normalizar(request.usuario, 30);
            string cedula = Cr_SeguimientoTramites_Filtro_Normalizar(request.cedula, 20);
            string codigo = Cr_SeguimientoTramites_Filtro_Normalizar(request.codigo, 20);
            string? mensaje = Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Validar(
                usuario,
                cedula,
                codigo);

            if (mensaje is not null)
            {
                return DbHelper.CreateErrorResponse(
                    mensaje,
                    -2,
                    new CrSeguimientoTramitesRecepcionLineaContextoData());
            }

            ErrorDto<CrSeguimientoTramitesRecepcionLineaContextoData> response =
                DbHelper.WithConn(
                    _portalDb,
                    codEmpresa,
                    conn => Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Cargar(
                        conn,
                        usuario,
                        cedula,
                        codigo));

            if (response.Code != 0
                && string.Equals(
                    response.Description,
                    "No existe la persona o la línea indicada.",
                    StringComparison.Ordinal))
            {
                return DbHelper.CreateErrorResponse(
                    response.Description,
                    -2,
                    new CrSeguimientoTramitesRecepcionLineaContextoData());
            }

            return response;
        }

        private static string? Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Validar(
            string usuario,
            string cedula,
            string codigo)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return "Debe indicar el usuario.";
            }

            if (string.IsNullOrWhiteSpace(cedula))
            {
                return "- No se ha indicado el Asociado";
            }

            return string.IsNullOrWhiteSpace(codigo)
                ? "- No se ha indicado la Línea de Crédito"
                : null;
        }

        private static CrSeguimientoTramitesRecepcionLineaContextoData
            Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Cargar(
                IDbConnection conn,
                string usuario,
                string cedula,
                string codigo)
        {
            const string sql = """
                select
                    rtrim(C.codigo) as codigo,
                    rtrim(S.nombre) as nombre,
                    rtrim(isnull(S.EstadoActual, '')) as estado_actual,
                    rtrim(C.descripcion) as descripcion,
                    rtrim(isnull(C.moneda, '')) as cod_divisa,
                    rtrim(isnull(C.base_calculo, '')) as base_calculo,
                    isnull(C.id_comite, 0) as comite_id
                from socios S
                cross join catalogo C
                where S.cedula = @Cedula
                  and C.codigo = @Codigo;

                select rtrim(D.cod_destino) as idx, rtrim(D.descripcion) as itmx
                from catalogo_destinos D
                inner join catalogo_destinosASG A on D.cod_destino = A.cod_destino
                where A.codigo = @Codigo
                order by D.prioridad;

                select rtrim(T.garantia) as idx, rtrim(T.descripcion) as itmx
                from crd_catalogo_garantias A
                inner join crd_garantia_tipos T on A.garantia = T.garantia
                where A.codigo = @Codigo;

                select rtrim(G.cod_grupo) as idx, rtrim(G.descripcion) as itmx
                from catalogo_grupos G
                inner join catalogo_asignaGrp A on G.cod_grupo = A.cod_grupo
                where G.estado = 1
                  and A.codigo = @Codigo;

                exec spCrd_SGT_Bancos @Usuario, @Moneda;
                """;

            CrSeguimientoTramitesRecepcionLineaContextoRaw? encabezado;
            List<CrSeguimientoTramitesOpcionItem> destinos;
            List<CrSeguimientoTramitesOpcionItem> garantias;
            List<CrSeguimientoTramitesOpcionItem> recursos;
            List<CrSeguimientoTramitesOpcionItem> bancos;

            using (SqlMapper.GridReader grid = conn.QueryMultiple(
                sql,
                new
                {
                    Usuario = usuario,
                    Cedula = cedula,
                    Codigo = codigo,
                    Moneda = Cr_SeguimientoTramites_Recepcion_Linea_Moneda_Obtener(
                        conn,
                        codigo)
                }))
            {
                encabezado = grid.ReadFirstOrDefault<
                    CrSeguimientoTramitesRecepcionLineaContextoRaw>();
                destinos = Cr_SeguimientoTramites_Opciones_Mapear(
                    grid.Read<CrSeguimientoTramitesOpcionRaw>());
                garantias = Cr_SeguimientoTramites_Opciones_Mapear(
                    grid.Read<CrSeguimientoTramitesOpcionRaw>());
                recursos = Cr_SeguimientoTramites_Opciones_Mapear(
                    grid.Read<CrSeguimientoTramitesOpcionRaw>());
                bancos = Cr_SeguimientoTramites_Opciones_Mapear(
                    grid.Read<CrSeguimientoTramitesOpcionRaw>());
            }

            if (encabezado is null)
            {
                throw new InvalidOperationException("No existe la persona o la línea indicada.");
            }

            return Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Mapear(
                encabezado,
                destinos,
                garantias,
                recursos,
                bancos);
        }

        private static string Cr_SeguimientoTramites_Recepcion_Linea_Moneda_Obtener(
            IDbConnection conn,
            string codigo)
        {
            return conn.QueryFirstOrDefault<string>(
                "select rtrim(isnull(moneda, '')) from catalogo where codigo = @Codigo;",
                new { Codigo = codigo }) ?? string.Empty;
        }

        private static CrSeguimientoTramitesRecepcionLineaContextoData
            Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Mapear(
                CrSeguimientoTramitesRecepcionLineaContextoRaw encabezado,
                List<CrSeguimientoTramitesOpcionItem> destinos,
                List<CrSeguimientoTramitesOpcionItem> garantias,
                List<CrSeguimientoTramitesOpcionItem> recursos,
                List<CrSeguimientoTramitesOpcionItem> bancos)
        {
            return new CrSeguimientoTramitesRecepcionLineaContextoData
            {
                codigo = encabezado.codigo.Trim(),
                nombre = encabezado.nombre.Trim(),
                estado_actual = encabezado.estado_actual.Trim(),
                descripcion = encabezado.descripcion.Trim(),
                cod_divisa = encabezado.cod_divisa.Trim(),
                base_calculo = encabezado.base_calculo.Trim(),
                comite_id = encabezado.comite_id,
                muestra_vencimiento = string.Equals(
                    encabezado.base_calculo.Trim(),
                    "07",
                    StringComparison.Ordinal),
                destinos = destinos,
                garantias = garantias,
                recursos = recursos,
                bancos = bancos,
                estados = Cr_SeguimientoTramites_Estados_Crear("R")
            };
        }
    }
}
