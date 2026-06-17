using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrConstanciasBL
    {
        private readonly FrmCrConstanciasDB _db;

        public FrmCrConstanciasBL(IConfiguration config)
        {
            _db = new FrmCrConstanciasDB(config);
        }

        public ErrorDto<CrConstanciasInicialDto> CR_Constancias_Inicial_Obtener(
            int CodEmpresa,
            string cedula,
            string nombre,
            DateTime? corte,
            string usuario)
        {
            return _db.CR_Constancias_Inicial_Obtener(CodEmpresa, cedula, nombre, corte, usuario);
        }

        public ErrorDto<List<CrConstanciasEducacionDto>> CR_Constancias_Educacion_List_Obtener(
            int CodEmpresa,
            string tipo,
            string? codigo)
        {
            return _db.CR_Constancias_Educacion_List_Obtener(CodEmpresa, tipo, codigo);
        }

        public ErrorDto<CrConstanciasPadronDto> CR_Constancias_Padron_Nombre_Obtener(
            int CodEmpresa,
            string identificacion)
        {
            return _db.CR_Constancias_Padron_Nombre_Obtener(CodEmpresa, identificacion);
        }

        public ErrorDto CR_Constancias_Bitacora_Registra(
            int CodEmpresa,
            CrConstanciasBitacoraRequest request)
        {
            return _db.CR_Constancias_Bitacora_Registra(CodEmpresa, request);
        }
        public ErrorDto<List<CrConstanciasPadronBusquedaDto>> CR_Constancias_Padron_Buscar(int CodEmpresa)
        {
            return _db.CR_Constancias_Padron_Buscar(CodEmpresa);
        }
    }
}