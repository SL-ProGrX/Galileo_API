using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo_API.DataBaseTier.ProGrX_Nucleo;

namespace Galileo_API.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysFacturaElectronicaBL
    {
        private readonly FrmSysFacturaElectronicaDB Db;

        public FrmSysFacturaElectronicaBL(IConfiguration config)
        {
            Db = new FrmSysFacturaElectronicaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FE_Clientes_DropDown_Obtener(int CodEmpresa)
        {
            return Db.FE_Clientes_DropDown_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Cabys_DropDown_Obtener(int CodEmpresa)
        {
            return Db.FE_Cabys_DropDown_Obtener(CodEmpresa);
        }
        public ErrorDto<FeCortesLista> FE_Cortes_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return Db.FE_Cortes_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<FeCortesLista> FE_Cortes_Lista_Export(int CodEmpresa, string parametros)
        {
            return Db.FE_Cortes_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto FE_Corte_Registrar(int CodEmpresa, FeRegistrarCorteDto dto)
        {
            return Db.FE_Corte_Registrar(CodEmpresa, dto);
        }

        public ErrorDto<FeFacturasLista> FE_Facturas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return Db.FE_Facturas_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<FeFacturasLista> FE_Facturas_Lista_Export(int CodEmpresa, string parametros)
        {
            return Db.FE_Facturas_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<List<FeFacturaDetalleItem>> FE_Factura_Detalle_Obtener(int CodEmpresa, string codCliente, string idFactura)
        {
            return Db.FE_Factura_Detalle_Obtener(CodEmpresa, codCliente, idFactura);
        }

        public ErrorDto<FeFacturasResumen> FE_Facturas_Resumen_Obtener(int CodEmpresa, string parametros)
        {
            return Db.FE_Facturas_Resumen_Obtener(CodEmpresa, parametros);
        }
        public ErrorDto<FeFacturasResumen> FE_Facturas_Resumen_Export(int CodEmpresa, string parametros)
        {
            return Db.FE_Facturas_Resumen_Export(CodEmpresa, parametros);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Facturas_Estados_DropDown_Obtener(int CodEmpresa)
        {
            return Db.FE_Facturas_Estados_DropDown_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Personas_DropDown_Obtener(int CodEmpresa, string? filtro)
        {
            return Db.FE_Personas_DropDown_Obtener(CodEmpresa, filtro);
        }
        public ErrorDto<FeClientesLista> FE_Clientes_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return Db.FE_Clientes_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<FeClientesLista> FE_Clientes_Lista_Export(int CodEmpresa, string parametros)
        {
            return Db.FE_Clientes_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<FeConfiguracionModel> FE_Configuracion_Obtener(int CodEmpresa, string codigo)
        {
            return Db.FE_Configuracion_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto FE_Configuracion_Guardar(int CodEmpresa, FeConfiguracionGuardarDto dto)
        {
            return Db.FE_Configuracion_Guardar(CodEmpresa, dto);
        }

        public ErrorDto FE_Configuracion_Eliminar(int CodEmpresa, string codigo, string usuario)
        {
            return Db.FE_Configuracion_Eliminar(CodEmpresa, codigo, usuario);
        }
        public ErrorDto FE_Clientes_Sincronizar(int CodEmpresa, string cod_cliente, string usuario)
        {
            return Db.FE_Clientes_Sincronizar(CodEmpresa, cod_cliente, usuario);
        }
        public ErrorDto<List<FeExclusionItem>> FE_Exclusiones_Consulta(int CodEmpresa, string cod_cliente, string tipo)
        {
            return Db.FE_Exclusiones_Consulta(CodEmpresa, cod_cliente, tipo);
        }

        public ErrorDto FE_Exclusion_Procesar(int CodEmpresa, string cod_cliente, string codigo, string movimiento, string tipo, string usuario)
        {
            return Db.FE_Exclusion_Procesar(CodEmpresa, cod_cliente, codigo, movimiento, tipo, usuario);
        }

        public ErrorDto FE_Reactivacion_Ejecutar(int CodEmpresa, DateTime fecha_inicio, DateTime fecha_corte, string usuario)
        {
            return Db.FE_Reactivacion_Ejecutar(CodEmpresa, fecha_inicio, fecha_corte, usuario);
        }
    }
}
