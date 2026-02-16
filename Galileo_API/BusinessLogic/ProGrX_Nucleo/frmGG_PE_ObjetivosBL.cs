using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GG_PE;

namespace Galileo.BusinessLogic
{
    public class FrmGgPeObjetivosBL(IConfiguration config)
    {
        private readonly FrmGgPeObjetivosDb _db = new(config);

        public ErrorDto<PeObjetivosEstrategicosDatosLista> PeObjetivosEstrategicosLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            return _db.PeObjetivosEstrategicosLista_Obtener(CodEmpresa, Jfiltros);
        }

        public ErrorDto ObjetivosEstrategicos_Guardar(int CodEmpresa, PeObjetivosEstrategicosDto objetivo)
        {
            return _db.ObjetivosEstrategicos_Guardar(CodEmpresa, objetivo);
        }

        public ErrorDto ObjetivosEstrategicos_Eliminar(int CodEmpresa, int objetivo_id)
        {
            return _db.ObjetivosEstrategicos_Eliminar(CodEmpresa, objetivo_id);
        }

        public ErrorDto<List<PeObjetivosEstrategicosDto>> PePerspectivaLista_Obtener(int CodEmpresa)
        {
            return _db.PePerspectivaLista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<PeObjetivosEstrategicosDto>> PeObservacionesExportar_Obtener(int CodEmpresa)
        {
            return _db.PeObservacionesExportar_Obtener(CodEmpresa);
        }

    }
}