using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesGeneraAsientosBL
    {
        private readonly FrmTesGeneraAsientosDB GeneraAsientosDb;
        private readonly MTesoreria mTesoreria;

        public FrmTesGeneraAsientosBL(IConfiguration config)
        {
            GeneraAsientosDb = new FrmTesGeneraAsientosDB(config);
            mTesoreria = new MTesoreria(config);

        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_Bancos_Obtener(int CodEmpresa, string usuario)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, "Asientos");
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_Tipos_Obtener(int CodEmpresa, string usuario, int cod_Banco)
        {
            return mTesoreria.sbTesTiposDocsCargaCboAcceso(CodEmpresa, usuario, cod_Banco, "X");
        }

        public ErrorDto<TablasListaGenericaModel> TES_transaccionesAsientos_Obtener(int CodEmpresa, string filtrosTransacciones, string filtros)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
            return GeneraAsientosDb.TES_transaccionesAsientos_Obtener(CodEmpresa, filtrosTransacciones, filtro);
        }

        public ErrorDto TES_Traslado_Generar(int CodEmpresa, string trasladoLista)
        {
            return GeneraAsientosDb.TES_Traslado_Generar(CodEmpresa, trasladoLista);
        }


    }
}