using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasRoeBL
    {
        private readonly FrmCajasRoeDb _db;
        public FrmCajasRoeBL(IConfiguration config)
        {
            _db = new FrmCajasRoeDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeTiposIds_Obtener(int cod_empresa)
        {
            return _db.Cajas_RoeTiposIds_Obtener(cod_empresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoePaises_Obtener(int cod_empresa)
        {
            return _db.Cajas_RoePaises_Obtener(cod_empresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeProvinciasPorPais_Obtener(int cod_empresa,string cod_pais)
        {
            return _db.Cajas_RoeProvinciasPorPais_Obtener(cod_empresa, cod_pais);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeCantonesPorProvincia_Obtener(int cod_empresa,string provincia)
        {
            return _db.Cajas_RoeCantonesPorProvincia_Obtener(cod_empresa, provincia);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_RoeDistritosPorProvinciaCanton_Obtener(int cod_empresa,string provincia,string canton)
        {
           return _db.Cajas_RoeDistritosPorProvinciaCanton_Obtener(cod_empresa, provincia, canton);
        }

        public ErrorDto<CajasRoeModelDto> Cajas_RoePorId_Obtener(int cod_empresa,int id_roe)
        {
            return _db.Cajas_RoePorId_Obtener(cod_empresa, id_roe);
        }

        public ErrorDto<int> Cajas_Roe_Imprime(int cod_empresa,int id_roe)
        {
            return _db.Cajas_Roe_Imprime(cod_empresa, id_roe);
        }

        public ErrorDto<SpResultadoModel> Cajas_Roe_Actualizar(int cod_empresa,CajasRoeActualizaParamsModel p)
        {
            return _db.Cajas_Roe_Actualizar(cod_empresa, p);
        }

        public ErrorDto<SpResultadoModel> Cajas_Roe_spImprime_Ejecutar(int cod_empresa,CajasRoeImprimeParamsModel p)
        {
            return _db.Cajas_Roe_spImprime_Ejecutar(cod_empresa, p);
        }
    }
}