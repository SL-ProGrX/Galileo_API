using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrX_Personas
{
    public class FrmAFAfiliacionTagsBL
    {
        private readonly FrmAFAfiliacionTagsDB _db;

        public FrmAFAfiliacionTagsBL(IConfiguration config)
        {
            _db = new FrmAFAfiliacionTagsDB(config);
        }

        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Recepcion(int CodEmpresa, string estado, string filtro)
        {
            return _db.AFI_Afiliaciones_Consulta_Recepcion(CodEmpresa, estado, filtro);
        }

        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Recibidas(int CodEmpresa, string estado, string filtro)
        {
            return _db.AFI_Afiliaciones_Consulta_Recibidas(CodEmpresa, estado, filtro);
        }

        public ErrorDto<List<AfiAfiliacionControlDto>> AFI_Afiliaciones_Consulta_Pendientes(int CodEmpresa, string estado, string filtro)
        {
            return _db.AFI_Afiliaciones_Consulta_Pendientes(CodEmpresa, estado, filtro);
        }

        public ErrorDto<List<AfBoletasAfiliacion>> AF_CR_BoletasAfiliacion_Obtener(int CodEmpresa)
        {
            return _db.AF_CR_BoletasAfiliacion_Obtener(CodEmpresa);
        }

        public ErrorDto AFI_Afiliacion_Recepcion_Aplica(int codEmpresa, int boleta, string usuario)
        {
            return _db.AFI_Afiliacion_Recepcion_Aplica(codEmpresa, boleta, usuario);
        }

        public ErrorDto AFI_Afiliacion_Revision_Aplica(int codEmpresa, int consec, string estado, string usuario, string nota)
        {
            return _db.AFI_Afiliacion_Revision_Aplica(codEmpresa, consec, estado, usuario, nota);
        }

        public ErrorDto<List<AfiEtiquetaDto>> AFI_Afiliaciones_Etiquetas_Consulta(int CodEmpresa, int boleta)
        {
            return _db.AFI_Afiliaciones_Etiquetas_Consulta(CodEmpresa, boleta);
        }

        public ErrorDto AFI_Afiliacion_Revision_Reversar(int CodEmpresa, int boleta, string usuario, string nota)
        {
            return _db.AFI_Afiliacion_Revision_Reversar(CodEmpresa, boleta, usuario, nota);
        }

        public ErrorDto AFI_Afiliacion_Recepcion_Agregar(int CodEmpresa, int boleta, string usuario)
        {
            return _db.AFI_Afiliacion_Recepcion_Agregar(CodEmpresa, boleta, usuario);
        }

        public ErrorDto<List<AfBoletasAfiliacion>> AF_BoletasAfiliacionLista_Obtener(int CodEmpresa)
        {
            return _db.AF_BoletasAfiliacionLista_Obtener(CodEmpresa);
        }

    }
}
