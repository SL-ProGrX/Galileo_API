using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrPrendasDb
    {
        /// <summary>
        /// Obtiene los tipos de prenda activos y la configuración que define su formulario.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa cuya base de datos se consulta.</param>
        /// <returns>Tipos activos con formulario y porcentaje de cobertura.</returns>
        public ErrorDto<List<CrPrendaTipoListaData>> CrPrendas_TiposActivos(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(tipo_prenda) as idx,
                    rtrim(descripcion) as itmx,
                    rtrim(isnull(formulario, '')) as formulario,
                    isnull(porc_cobertura, 0) as porc_cobertura
                from crd_prendas_tipos
                where Activa = 1
                order by descripcion;";

            return DbHelper.ExecuteListQuery<CrPrendaTipoListaData>(_portalDb, codEmpresa, sql);
        }

        /// <summary>Obtiene un catálogo auxiliar de garantías prendarias.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="tipoCatalogo">Tipo de catálogo solicitado.</param>
        /// <returns>Elementos disponibles del catálogo.</returns>
        public ErrorDto<List<CrPrendaTipoListaData>> CrPrendas_CatalogoLista(int codEmpresa, string tipoCatalogo)
        {
            const string sql = "exec spCrd_Prendas_Cat_List_Cbo @TipoCatalogo;";

            return DbHelper.ExecuteListQuery<CrPrendaTipoListaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { TipoCatalogo = (tipoCatalogo ?? string.Empty).Trim().ToUpperInvariant() });
        }

        /// <summary>Obtiene las unidades de medida aplicables a la característica indicada.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="aplicacion">Aplicación: peso, capacidad o cilindraje.</param>
        /// <returns>Unidades activas aplicables.</returns>
        public ErrorDto<List<CrPrendaTipoListaData>> CrPrendas_UnidadesLista(int codEmpresa, string aplicacion)
        {
            var campo = (aplicacion ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "PESO" => "Peso_Apl",
                "CAPACIDAD" => "Capacidad_Apl",
                "CILINDRAJE" => "Cilindraje_Apl",
                _ => "Peso_Apl"
            };

            var sql = $@"
                select rtrim(ID_Unidad) as idx, rtrim(descripcion) as itmx
                from CRD_PRENDAS_uds
                where {campo} = 1 and Activa = 1
                order by Descripcion;";

            return DbHelper.ExecuteListQuery<CrPrendaTipoListaData>(_portalDb, codEmpresa, sql);
        }

        /// <summary>Obtiene los parentescos activos para la persona asegurada.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Parentescos activos.</returns>
        public ErrorDto<List<CrPrendaTipoListaData>> CrPrendas_ParentescosLista(int codEmpresa)
        {
            const string sql = @"
                select rtrim(cod_Parentesco) as idx, rtrim(Descripcion) as itmx
                from sys_Parentescos
                where activo = 1
                order by Descripcion;";

            return DbHelper.ExecuteListQuery<CrPrendaTipoListaData>(_portalDb, codEmpresa, sql);
        }

        /// <summary>Obtiene los tipos de identificación de personas físicas.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Tipos de identificación y su longitud mínima.</returns>
        public ErrorDto<List<CrPrendaTipoListaData>> CrPrendas_TiposIdentificacionLista(int codEmpresa)
        {
            const string sql = @"
                select
                    convert(varchar(10), TIPO_ID) as idx,
                    rtrim(Descripcion) as itmx,
                    isnull(LARGO_MINIMO, 0) as largo_minimo
                from AFI_TIPOS_IDS
                where TIPO_PERSONERIA = 'F'
                order by Tipo_Id;";

            return DbHelper.ExecuteListQuery<CrPrendaTipoListaData>(_portalDb, codEmpresa, sql);
        }

        /// <summary>Obtiene las anotaciones registradas para una prenda.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="prendaId">Identificador de la prenda.</param>
        /// <returns>Anotaciones de la prenda.</returns>
        public ErrorDto<List<CrPrendaAnotacionData>> CrPrendas_AnotacionesLista(int codEmpresa, long prendaId)
        {
            const string sql = "exec spCrd_Prendas_Anotaciones_Lista @PrendaId;";

            return EjecutarListaMapeada(codEmpresa, sql, new { PrendaId = prendaId }, MapearAnotacion);
        }

        /// <summary>Obtiene las coberturas de póliza configuradas para una prenda.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="tipoPrenda">Código del tipo de prenda.</param>
        /// <param name="prendaId">Identificador de la prenda.</param>
        /// <returns>Coberturas disponibles y asignadas.</returns>
        public ErrorDto<List<CrPrendaPolizaCoberturaData>> CrPrendas_PolizasList(int codEmpresa, string tipoPrenda, long prendaId)
        {
            const string sql = "exec spCrd_Prendas_Polizas_List @TipoPrenda, @PrendaId;";

            return EjecutarListaMapeada(
                codEmpresa,
                sql,
                new { TipoPrenda = tipoPrenda ?? string.Empty, PrendaId = prendaId },
                MapearPolizaCobertura);
        }

        /// <summary>Obtiene el historial de avalúos de una prenda.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="prendaId">Identificador de la prenda.</param>
        /// <returns>Avalúos históricos registrados.</returns>
        public ErrorDto<List<CrPrendaHistoricoAvaluoData>> CrPrendas_AvaluosLista(int codEmpresa, long prendaId)
        {
            const string sql = "exec spCrd_Prendas_Avaluos_Lista @PrendaId;";

            return EjecutarListaMapeada(codEmpresa, sql, new { PrendaId = prendaId }, MapearHistoricoAvaluo);
        }
    }
}
