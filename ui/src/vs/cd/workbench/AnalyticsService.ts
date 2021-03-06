//    Copyright CodeMerx 2020
//    This file is part of CodemerxDecompile.

//    CodemerxDecompile is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    CodemerxDecompile is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.

//    You should have received a copy of the GNU Affero General Public License
//    along with CodemerxDecompile.  If not, see<https://www.gnu.org/licenses/>.

import { createDecorator } from 'vs/platform/instantiation/common/instantiation';
import { IMainProcessService } from 'vs/platform/ipc/electron-sandbox/mainProcessService';
import { createChannelSender } from 'vs/base/parts/ipc/common/ipc';

export const IAnalyticsService = createDecorator<IAnalyticsService>('IAnalyticsService');

export interface IAnalyticsService {
	readonly _serviceBrand: undefined;

	trackEvent(category: string, action: string, label?: string, value?: string | number): Promise<void>;
	trackException(description: string): Promise<void>;
}

export class AnalyticsService {
	readonly _serviceBrand: undefined;

	constructor(@IMainProcessService mainProcessService: IMainProcessService) {
		return createChannelSender<IAnalyticsService>(mainProcessService.getChannel('analyticsService'));
	}
}
