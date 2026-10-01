using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAHExcedentesDistribucionBL
    {
        private readonly FrmAHExcedentesDistribucionDB _db;

        public FrmAHExcedentesDistribucionBL(IConfiguration config)
        {
            _db = new FrmAHExcedentesDistribucionDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            AH_Excedentes_Distribucion_Periodos_Lista_Obtener(int CodEmpresa)
        {
            return _db.AH_Excedentes_Distribucion_Periodos_Lista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            AH_Excedentes_Distribucion_Cortes_Lista_Obtener(int CodEmpresa, string periodo)
        {
            return _db.AH_Excedentes_Distribucion_Cortes_Lista_Obtener(CodEmpresa, periodo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            AH_Excedentes_Distribucion_Tipos_Lista_Obtener(int CodEmpresa, string usuario)
        {
            return _db.AH_Excedentes_Distribucion_Tipos_Lista_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<AHExcMontoListadoDto>>
            AH_Excedentes_Distribucion_Montos_Lista_Obtener(int CodEmpresa, string periodo)
        {
            return _db.AH_Excedentes_Distribucion_Montos_Lista_Obtener(CodEmpresa, periodo);
        }

        public ErrorDto<decimal> AH_Excedentes_Distribucion_Monto_Calculo_Obtener(int CodEmpresa, string periodo, string corte,string tipo,string baseCalculo,decimal porcentaje)
        {
            return _db.AH_Excedentes_Distribucion_Monto_Calculo_Obtener(CodEmpresa,periodo,corte,tipo,baseCalculo,porcentaje);
        }

        public ErrorDto<bool> AH_Excedentes_Distribucion_Aplicada_Obtener(int CodEmpresa,string periodo,string corte)
        {
            return _db.AH_Excedentes_Distribucion_Aplicada_Obtener(CodEmpresa,periodo,corte);
        }

        public ErrorDto AH_Excedentes_Distribucion_Monto_Guardar(int CodEmpresa,string usuario,AHExcMontoDto dto)
        {
            return _db.AH_Excedentes_Distribucion_Monto_Guardar(CodEmpresa,usuario,dto);
        }

        public ErrorDto AH_Excedentes_Distribucion_Monto_Eliminar(int CodEmpresa, string usuario, AHExcMontoDto dto)
        {
            return _db.AH_Excedentes_Distribucion_Monto_Eliminar(CodEmpresa, usuario, dto);
        }
    }
}