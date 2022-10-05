﻿using Cwru.Common;
using Cwru.Common.Config;
using Cwru.Common.Model;
using Cwru.Connection.Services;
using Cwru.Publisher.Services;
using Cwru.VsExtension.Commands.Base;
using System.Threading.Tasks;

namespace Cwru.VsExtension.Commands
{

    internal sealed class UpdateWrEnvironmentsCommand : PublisherCommandBase
    {
        private readonly UpdateWrService updateWrService;

        public UpdateWrEnvironmentsCommand(Logger logger, ConnectionService connectionService, UpdateWrService updateWrService) : base(logger, connectionService)
        {
            this.updateWrService = updateWrService;
        }

        protected override async Task ExecutePublisherLogicAsync(ProjectInfo projectInfo, ProjectConfig projectConfig)
        {
            await updateWrService.UploadWrEnvironmentsAsync(projectInfo, projectConfig, true);
        }
    }
}
