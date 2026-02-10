
namespace Galileo.Models.ProGrX_Nucleo
{
        public class FeCorteItem
        {
            public int corte_id { get; set; }
            public DateTime? corte { get; set; }
            public DateTime? facturacion { get; set; }
            public string? metodo { get; set; }
            public string? reg_usuario { get; set; }
            public DateTime? reg_fecha { get; set; }
        }
        public class FeCortesLista
        {
            public int total { get; set; }
            public List<FeCorteItem> lista { get; set; } = new();
        }
        public class FeRegistrarCorteDto
        {
            public string cod_cliente { get; set; } = string.Empty;
            public string fecha_corte { get; set; } = string.Empty;
            public string fecha_factura { get; set; } = string.Empty;
            public string usuario { get; set; } = string.Empty;
        }
        public class FeProcesarCorteDto
        {
            public string? cod_cliente { get; set; }    
            public string? fecha_corte { get; set; }  
            public string? fecha_factura { get; set; }  
            public string? usuario { get; set; }        
        }
    public class FeClienteInfo
        {
            public string? identificacion { get; set; }
            public string? nombre { get; set; }
        }
        public class FeFacturaItem
        {
            public string? tipo { get; set; }
            public string? comprobante { get; set; }
            public string? identificacion { get; set; }
            public string? razon_social { get; set; }
            public DateTime? fecha { get; set; }
            public decimal total { get; set; }
            public decimal total_exento { get; set; }
            public decimal total_gravado { get; set; }
            public decimal total_impuestos { get; set; }
            public decimal total_descuentos { get; set; }
            public decimal total_comprobante { get; set; }
            public string? clave { get; set; }
            public string? xml_respuesta { get; set; }
            public string? observaciones { get; set; }
            public int id_factura { get; set; }
        }
        public class FeFacturasLista
        {
            public int total { get; set; }
            public List<FeFacturaItem> lista { get; set; } = new();
        }
        public class FeFacturaDetalleItem
        {
            public int linea { get; set; }
            public string? codigo { get; set; }
            public string? producto { get; set; }
            public decimal precio_ud { get; set; }
            public decimal qty { get; set; }
            public string? unidad { get; set; }
            public decimal total { get; set; }
            public decimal descuento { get; set; }
            public decimal impuesto { get; set; }
            public string? cabys { get; set; }
        }
        public class FeFacturasResumenCabecera
        {
            public int no_facturas { get; set; }
            public DateTime? inicio { get; set; }
            public DateTime? corte { get; set; }
            public decimal monto_facturado { get; set; }
        }
        public class FeFacturaResumenItem
        {
            public string? tipo { get; set; }
            public int lineas { get; set; }
            public string? detalle { get; set; }
            public decimal facturado { get; set; }
            public DateTime? inicio { get; set; }
            public DateTime? corte { get; set; }
            public string? xml_respuesta { get; set; }
        }
        public class FeFacturasResumen
        {
            public FeFacturasResumenCabecera cabecera { get; set; } = new();
            public List<FeFacturaResumenItem> lista { get; set; } = new();
        }
        public class FeClienteItem
        {
            public string? id_prov { get; set; }
            public string? tipo_id { get; set; }
            public string? identificacion { get; set; }
            public string? razon_social { get; set; }
            public string? email1 { get; set; }
            public string? email2 { get; set; }
            public string? telefono1 { get; set; }
            public string? telefono2 { get; set; }
            public string? provincia { get; set; }
            public string? canton { get; set; }
            public string? distrito { get; set; }
            public string? barrio { get; set; }
            public string? direccion { get; set; }
        }
        public class FeClientesLista
        {
            public int total { get; set; }
            public List<FeClienteItem> lista { get; set; } = new();
        }
        public class FeExclusionItem
        {
            public string? codigo { get; set; }
            public string? descripcion { get; set; }
            public DateTime? registro_fecha { get; set; }
            public string? registro_usuario { get; set; }
            public int asignado { get; set; }
        }
        public class FeFacturasParametrosDto
        {
            public string cod_cliente { get; set; } = "";
            public string identificacion { get; set; } = "";
            public string nombre { get; set; } = "";
            public string factura { get; set; } = "";
            public string fecha_inicio { get; set; } = "";
            public string fecha_corte { get; set; } = "";
            public string estado { get; set; } = "";
        }
        public class FeConfiguracionGuardarDto
        {
            public string? codigo { get; set; }              
            public string? tipo_id { get; set; }             
            public string? identificacion { get; set; }      
            public string? razon_social { get; set; }        
            public short activa { get; set; }                
            public string? inicio { get; set; }              
            public string? notifica_email { get; set; }      
            public short notifica_activa { get; set; }       
            public short notifica_cliente { get; set; }      

            public int consec_fe { get; set; }               
            public int? consec_nc { get; set; }               
            public int? consec_nd { get; set; }               
            public int? consec_te { get; set; }               

            public string? portal_codigo { get; set; }       
            public string? portal_server { get; set; }       
            public string? portal_db { get; set; }          
            public string? portal_user { get; set; }         
            public string? portal_key { get; set; }          

            public string? metodo { get; set; }              
            public short? i_polizas { get; set; }             
            public short? i_principal { get; set; }          

            public short? mnt_max_apl { get; set; }          
            public decimal? mnt_max { get; set; }            

            public string? cabys { get; set; }               
            public string? sucursal { get; set; }            
            public string? terminal { get; set; }            

            public string? usuario { get; set; }             
        }

        public class FeConfiguracionModel
        {
            public string? codigo { get; set; }              
            public string? tipo_id { get; set; }            
            public string? identificacion { get; set; }      
            public string? razon_social { get; set; }
            public short? activa { get; set; }
            public DateTime? inicio { get; set; }            

            public string? notifica_email { get; set; }
            public short? notifica_activa { get; set; }
            public short? notifica_cliente { get; set; }
            public int? consec_fe { get; set; }               
            public int? consec_nc { get; set; }               
            public int consec_nd { get; set; }               
            public int consec_te { get; set; }               
            public string? portal_codigo { get; set; }      
            public string? portal_server { get; set; }       
            public string? portal_db { get; set; }           
            public string? portal_user { get; set; }         
            public string? portal_key { get; set; }        

            public string? metodo { get; set; }              
            public short i_polizas { get; set; }            
            public short i_principal { get; set; }          

            public short mnt_max_apl { get; set; }          
            public decimal mnt_max { get; set; }            

            public string? cabys { get; set; }               
            public string? sucursal { get; set; }           
            public string? terminal { get; set; }
            public string? cabys_desc { get; set; }
    }

}
