using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.DataBaseTier.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosDeteriorosBl
    {
        private readonly FrmActivosDeteriorosDb _db;

        public FrmActivosDeteriorosBl(IConfiguration config)
        {
            _db = new FrmActivosDeteriorosDb(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Deterioros_Justificaciones_Obtener(int CodEmpresa)
        {
            return _db.Activos_Deterioros_Justificaciones_Obtener(CodEmpresa);
        }
        public ErrorDto<List<ActivosData>> Activos_Deterioros_Activos_Obtener(int CodEmpresa)
        {
            return _db.Activos_Deterioros_Activos_Obtener(CodEmpresa);
        }
        public ErrorDto<ActivosDeterioroData?> Activos_Deterioros_Consultar(int CodEmpresa, int Id_AddRet, string placa)
        {
            return _db.Activos_Deterioros_Consultar(CodEmpresa, Id_AddRet, placa);
        }
        public ErrorDto<string> Activos_Deterioros_Validar(int CodEmpresa, string placa, DateTime fecha)
        {
            return _db.Activos_Deterioros_Validar(CodEmpresa,  placa, fecha);
        }
      
        public ErrorDto<ActivosDeterioroDetallaData> Activos_DeteriorosDetalle_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_DeteriorosDetalle_Consultar(CodEmpresa, placa);
        }
       
     
        public ErrorDto Activos_Deterioros_Guardar(int CodEmpresa, string usuario, ActivosDeterioroData data)
        {
            return _db.Activos_Deterioros_Guardar(CodEmpresa, usuario, data);
        }
        public ErrorDto<List<ActivosHistoricoData>> Activos_Deterioros_Historico_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_Deterioros_Historico_Consultar(CodEmpresa, placa);
        } 
        public ErrorDto<string?> Activos_Deterioros_ActivosNombre_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_Deterioros_ActivosNombre_Consultar(CodEmpresa, placa);
        }
        public ErrorDto Activos_Deterioros_Eliminar(int CodEmpresa, string usuario, string placa, int Id_AddRet)
        {
            return _db.Activos_Deterioros_Eliminar(CodEmpresa, usuario, placa, Id_AddRet);
        }
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            return _db.Activos_Periodo_Consultar(CodEmpresa, contabilidad);
        }
    }
}
