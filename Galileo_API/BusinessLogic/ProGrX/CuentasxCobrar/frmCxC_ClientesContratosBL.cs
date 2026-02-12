
using Galileo.Models;
using Galileo.Models.ERROR; 
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCClientesContratosModels;


namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCClientesContratosBL
    {

        private readonly FrmCxCClientesContratosDb _db;

        public FrmCxCClientesContratosBL(IConfiguration config) => _db = new FrmCxCClientesContratosDb(config);

        public ErrorDto<string> CxCClientesPersonas_Contratos_Consultar(int codEmpresa, string cedula, int orden, string contrato)
        {             
            return _db.CxCClientesPersonas_Contratos_Consultar(codEmpresa, cedula,orden,contrato);
        }
        public ErrorDto<ClientesContratosData> CxCContratos_Consultar(int codEmpresa, string cedula, string contrato)
        {
            return _db.CxCContratos_Consultar(codEmpresa, cedula,contrato);
        }
        public ErrorDto CxCClientesPersonasContratos_Guardar(int codEmpresa, string usuario, ClientesContratosData datos)
        {

            return _db.CxCClientesPersonasContratos_Guardar(codEmpresa, usuario,datos);
        }
        public ErrorDto CxCClientesPersonas_Contratos_Eliminar(int codEmpresa, string usuario, string cedula, string contrato)
        {
            return _db.CxCClientesPersonas_Contratos_Eliminar(codEmpresa, usuario, cedula, contrato);
        }
        public ErrorDto CxCClientesPersonas_ContratosSuscripciones_Insertar(int CodEmpresa, string usuario, PersonasContratosSuscripcionesData datos)
        {
            return _db.CxCClientesPersonas_ContratosSuscripciones_Insertar(CodEmpresa, usuario, datos);
        }
        public ErrorDto CxCClientesPersonas_ContratosSuscripciones_Eliminar(int CodEmpresa, string usuario, string cedula, string contrato, string cargo)
        {
            return _db.CxCClientesPersonas_ContratosSuscripciones_Eliminar(CodEmpresa, usuario, cedula, contrato, cargo);
        }
        public ErrorDto CxCClientesPersonas_ContratosPagadores_Eliminar(int CodEmpresa, string usuario, string cedula, string contrato, string cedula_pagador)
        {
            return _db.CxCClientesPersonas_ContratosPagadores_Eliminar(CodEmpresa, usuario, cedula, contrato, cedula_pagador);
        }
        public ErrorDto CxCClientesPersonas_ContratosPagadores_Insertar(int CodEmpresa, string usuario, PersonasContratosPagadoresData datos)
        {
            return _db.CxCClientesPersonas_ContratosPagadores_Insertar(CodEmpresa, usuario, datos);
        }
        public ErrorDto<List<PersonasContratosPagadoresData>> CxCClientesPersonas_ContratosPagadores_Lista(int CodEmpresa, string cedula, string contrato)
        {
            return _db.CxCClientesPersonas_ContratosPagadores_Lista(CodEmpresa, cedula, contrato);
        } 
        public ErrorDto<List<PersonasContratosSuscripcionesData>> CxCClientesPersonas_ContratosSuscripcion_Lista(int CodEmpresa, string cedula, string contrato)
        {
            return _db.CxCClientesPersonas_ContratosSuscripcion_Lista(CodEmpresa, cedula, contrato);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Contratos_Obtener(int CodEmpresa)
        {
            return _db.CxC_Contratos_Obtener(CodEmpresa);
        }
    }
}
