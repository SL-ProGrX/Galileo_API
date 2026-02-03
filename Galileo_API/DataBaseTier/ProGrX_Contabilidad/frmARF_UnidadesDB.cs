using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmArfUnidadesDb
    {
        private readonly PortalDB _portalDb;

        public FrmArfUnidadesDb(IConfiguration config)
            : this(new PortalDB(config)) { }

        public FrmArfUnidadesDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Provincias_Obtener(int codEmpresa)
        {
            string query = @"select Provincia as item, rtrim(Descripcion) as descripcion from Provincias";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Cantones_Obtener(int codEmpresa, string codProvincia)
        {
            string query = @"select Canton as item, rtrim(Descripcion) as descripcion from Cantones 
                where provincia = @codProvincia order by descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codProvincia });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Distritos_Obtener(int codEmpresa, string codProvincia, string codCanton)
        {
            string query = @"select Distrito as item, rtrim(Descripcion) as descripcion from Distritos 
                where provincia = @codProvincia and Canton = @codCanton order by descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codProvincia, codCanton });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_CentroCostos_Obtener(int codEmpresa, int codConta)
        {
            string query = @"select cod_centro_Costo as item,descripcion from Cntx_Centro_Costos where cod_Contabilidad = @codConta";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_CntxUnidades_Obtener(int codEmpresa)
        {
            string query = @"select cod_unidad as item,descripcion from vARF_UNIDADES";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ArfUnidades_Unidades_Obtener(int codEmpresa)
        {
            string query = @"select COD_LOCAL as item, Descripcion from ARF_UNIDADES";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<ArfUnidadesData> ArfUnidades_Scroll_Obtener(int codEmpresa, string codUnidad, int scrollCode)
        {
            const string query = @"
                select Top 1 COD_LOCAL from ARF_UNIDADES 
                WHERE
                    (
                        @scrollCode = 1 AND COD_LOCAL > @codUnidad
                    )
                    OR
                    (
                        @scrollCode <> 1 AND COD_LOCAL < @codUnidad
                    )
                ORDER BY
                    CASE WHEN @scrollCode = 1 THEN COD_LOCAL END ASC,
                    CASE WHEN @scrollCode <> 1 THEN COD_LOCAL END DESC;";

            var codResult = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, query, null, new { scrollCode, codUnidad });

            if (string.IsNullOrWhiteSpace(codResult?.Result))
            {
                return new ErrorDto<ArfUnidadesData>
                {
                    Code = -1,
                    Description = codResult?.Description,
                    Result = null
                };
            }

            return ArfUnidades_ConsultaUnidad_Obtener(codEmpresa, codResult.Result);
        }

        public ErrorDto<ArfUnidadesData> ArfUnidades_ConsultaUnidad_Obtener(int codEmpresa, string codUnidad)
        {
            string query = @"select P.*,rtrim(Prov.Descripcion) as ProvDesc, rtrim(Cant.Descripcion) as CantonDesc, rtrim(Dist.Descripcion) as DistDesc
                from ARF_UNIDADES P 
                left join Provincias Prov on P.Provincia = Prov.Provincia
                left join Cantones Cant on P.Provincia = Cant.Provincia and P.Canton = Cant.Canton
                left join Distritos Dist on P.Provincia = Dist.Provincia and P.Canton = Dist.Canton and P.distrito = Dist.distrito
                where P.COD_LOCAL = @codUnidad";
            var result = DbHelper.ExecuteSingleQuery(_portalDb, codEmpresa, query, new ArfUnidadesData(), new { codUnidad });
            result.Result ??= null;
            return result!;
        }
    }
}
