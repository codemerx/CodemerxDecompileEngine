/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

import { isMacintosh } from 'vs/base/common/platform';
/* AGPL */
import { CopyAction } from 'vs/editor/contrib/clipboard/clipboard';
/* End AGPL */
import { ServicesAccessor } from 'vs/platform/instantiation/common/instantiation';
import { getActiveNotebookEditor } from 'vs/workbench/contrib/notebook/browser/contrib/coreActions';
import { ElectronWebviewBasedWebview } from 'vs/workbench/contrib/webview/electron-browser/webviewElement';
import { IEditorService } from 'vs/workbench/services/editor/common/editorService';

function getFocusedElectronBasedWebviewDelegate(accessor: ServicesAccessor): ElectronWebviewBasedWebview | undefined {
	const editorService = accessor.get(IEditorService);
	const editor = getActiveNotebookEditor(editorService);
	if (!editor?.hasFocus()) {
		return;
	}

	const webview = editor?.getInnerWebview();
	if (webview && webview instanceof ElectronWebviewBasedWebview) {
		return webview;
	}
	return;
}

if (isMacintosh) {
	function withWebview(accessor: ServicesAccessor, f: (webviewe: ElectronWebviewBasedWebview) => void) {
		const webview = getFocusedElectronBasedWebviewDelegate(accessor);
		if (webview) {
			f(webview);
			return true;
		}
		return false;
	}

	const PRIORITY = 100;

	/* AGPL */
	// UndoCommand.addImplementation(PRIORITY, accessor => {
	// 	return withWebview(accessor, webview => webview.undo());
	// });

	// RedoCommand.addImplementation(PRIORITY, accessor => {
	// 	return withWebview(accessor, webview => webview.redo());
	// });
	/* End AGPL */

	CopyAction?.addImplementation(PRIORITY, accessor => {
		return withWebview(accessor, webview => webview.copy());
	});

	/* AGPL */
	// PasteAction?.addImplementation(PRIORITY, accessor => {
	// 	return withWebview(accessor, webview => webview.paste());
	// });

	// CutAction?.addImplementation(PRIORITY, accessor => {
	// 	return withWebview(accessor, webview => webview.cut());
	// });
	/* End AGPL */
}
