using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosRevaluacionesBL
    {
        private readonly FrmActivosRevaluacionesDB _db;

        public FrmActivosRevaluacionesBL(IConfiguration config)
        {
            _db = new FrmActivosRevaluacionesDB(config);
        }

        public ErrorDto Activos_Revaluaciones_Guardar(int CodEmpresa, string usuario, ActivosRevaluacionData data)
        {
            return _db.Activos_Revaluaciones_Guardar(CodEmpresa, usuario, data);
        }
     
        public ErrorDto<List<ActivosHistoricoData>> Activos_Revaluaciones_Historico_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_Revaluaciones_Historico_Consultar(CodEmpresa, placa);
        }

        public ErrorDto Activos_Revaluaciones_Eliminar(int CodEmpresa, string placa, int Id_AddRet, string usuario)
        {
            return _db.Activos_Revaluaciones_Eliminar(CodEmpresa, placa, Id_AddRet, usuario);
        }

    }
}