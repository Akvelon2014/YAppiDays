﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.33440
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YAppiWinService.YAppiWebService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="YAppiWebService.IYAppiAdminService")]
    public interface IYAppiAdminService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IYAppiAdminService/DoneAllOperations", ReplyAction="http://tempuri.org/IYAppiAdminService/DoneAllOperationsResponse")]
        bool DoneAllOperations(string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IYAppiAdminService/DoneAllOperations", ReplyAction="http://tempuri.org/IYAppiAdminService/DoneAllOperationsResponse")]
        System.Threading.Tasks.Task<bool> DoneAllOperationsAsync(string password);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IYAppiAdminServiceChannel : YAppiWinService.YAppiWebService.IYAppiAdminService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class YAppiAdminServiceClient : System.ServiceModel.ClientBase<YAppiWinService.YAppiWebService.IYAppiAdminService>, YAppiWinService.YAppiWebService.IYAppiAdminService {
        
        public YAppiAdminServiceClient() {
        }
        
        public YAppiAdminServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public YAppiAdminServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public YAppiAdminServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public YAppiAdminServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool DoneAllOperations(string password) {
            return base.Channel.DoneAllOperations(password);
        }
        
        public System.Threading.Tasks.Task<bool> DoneAllOperationsAsync(string password) {
            return base.Channel.DoneAllOperationsAsync(password);
        }
    }
}
