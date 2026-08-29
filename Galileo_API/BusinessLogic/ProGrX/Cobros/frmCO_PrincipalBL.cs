using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;


namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOPrincipalBL
    {
        private readonly FrmCOPrincipalDB _db;

        public FrmCOPrincipalBL(IConfiguration config)
        {
            _db = new FrmCOPrincipalDB(config);
        }

        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            return _db.Operaciones_Listar(codEmpresa);
        }

        public ErrorDto<OperacionConsultarDto> Operacion_Consultar(int codEmpresa, int operacion)
        {
            return _db.Operacion_Consultar(codEmpresa, operacion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Deductoras_Listar(int codEmpresa, int codInstitucion)
        {
            return _db.Deductoras_Listar(codEmpresa, codInstitucion);
        }

        public ErrorDto<CoEstadoDto> Estado_Consultar(int codEmpresa, int operacion, DateTime? fechaCorte)
        {
            return _db.Estado_Consultar(codEmpresa, operacion, fechaCorte);
        }

        public ErrorDto<List<CoHistorialDto>> Historial_Listar(int codEmpresa, int operacion)
        {
            return _db.Historial_Listar(codEmpresa, operacion);
        }

        public ErrorDto<List<COGestionDto>> Gestiones_Listar(int codEmpresa, string cedula)
        {
            return _db.Gestiones_Listar(codEmpresa, cedula);
        }

        public ErrorDto<List<COCobroFiadorRowDto>> CobroFiadores_Listar(int codEmpresa, int operacion)
        {
            return _db.CobroFiadores_Listar(codEmpresa, operacion);
        }

        public ErrorDto<string> CobroFiador_Cancelar(int codEmpresa, int operacion, string usuario)
        {
            return _db.CobroFiador_Cancelar(codEmpresa, operacion, usuario);
        }

        public ErrorDto<List<COTrasladoDeudaRowDto>> TrasladoDeuda_Listar(int codEmpresa, int operacion)
        {
            return _db.TrasladoDeuda_Listar(codEmpresa, operacion);
        }

        public ErrorDto<string> TrasladoDeuda_Revertir(int codEmpresa, COTrasladoDeudaRevertirRequestDto request)
        {
            return _db.TrasladoDeuda_Revertir(codEmpresa, request);
        }

        public ErrorDto<COContactoDto> Contacto_Consultar(int codEmpresa, int operacion)
        {
            return _db.Contacto_Consultar(codEmpresa, operacion);
        }

        public ErrorDto<List<COMoraDto>> Mora_Listar(int codEmpresa, int operacion, string tipo)
        {
            return _db.Mora_Listar(codEmpresa, operacion, tipo);
        }

        public ErrorDto<List<COEjecutivoDto>> Ejecutivos_Listar(int codEmpresa, int operacion)
        {
            return _db.Ejecutivos_Listar(codEmpresa, operacion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Lineas_Listar(int codEmpresa)
        {
            return _db.Lineas_Listar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Personas_Listar(int codEmpresa)
        {
            return _db.Personas_Listar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> LineasPorPersona_Listar(int codEmpresa, string cedula)
        {
            return _db.LineasPorPersona_Listar(codEmpresa, cedula);
        }

        public ErrorDto<List<OperacionBusquedaDto>> OperacionesPorPersonaLinea_Listar(int codEmpresa, string cedula, string linea)
        {
            return _db.OperacionesPorPersonaLinea_Listar(codEmpresa, cedula, linea);
        }

        public ErrorDto<string> CambiarDeductora(int codEmpresa, int operacion, int deductora)
        {
            return _db.CambiarDeductora(codEmpresa, operacion, deductora);
        }

        public ErrorDto<bool> ValidarCongelamiento(int codEmpresa, string cedula, string tipo)
        {
            return _db.ValidarCongelamiento(codEmpresa, cedula, tipo);
        }

        public ErrorDto<bool> ValidarPasoCobroJudicial(int codEmpresa, int operacion)
        {
            return _db.ValidarPasoCobroJudicial(codEmpresa, operacion);
        }

        public ErrorDto<string> CobroJudicial_Ejecutar(int codEmpresa, int operacion, string usuario, string notas)
        {
            return _db.CobroJudicial_Ejecutar(codEmpresa, operacion, usuario, notas);
        }

        public ErrorDto<List<COAvisoDto>> Avisos_Listar(int codEmpresa, int operacion)
        {
            return _db.Avisos_Listar(codEmpresa, operacion);
        }
    }
}
