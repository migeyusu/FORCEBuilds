﻿namespace FORCEBuild.Message.Base
{
    public class MessagePipeline
    {
        

        public MessagePipeline Add<T,K>(PipelineStage<T,K> stage)
        {
            
            return this;
        }   
    }
}