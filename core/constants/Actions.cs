// Copyright (c) 2025 Momspants Games
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace IrksomeIsland.Core.Constants;

public static class Actions
{
	public static class Movement
	{
		public static string Forward => "move_forward";
		public static string Backward => "move_backward";
		public static string Left => "move_left";
		public static string Right => "move_right";
	}

	public static class MovementAction
	{
		public static string Sprint => "sprint";
		public static string Jump => "jump";
	}

	public static class Camera
	{
		public static string RotateLeft => "cam_rotate_left";
		public static string RotateRight => "cam_rotate_right";
		public static string ZoomIn => "cam_zoom_in";
		public static string ZoomOut => "cam_zoom_out";
		public static string PitchUp => "cam_pitch_up";
		public static string PitchDown => "cam_pitch_down";
	}

	public static class Ui
	{
		public static string Accept => "ui_accept";
		public static string Select => "ui_select";
		public static string Cancel => "ui_cancel";
		public static string FocusNext => "ui_focus_next";
		public static string FocusPrev => "ui_focus_prev";

		public static string Left => "ui_left";
		public static string Right => "ui_right";
		public static string Up => "ui_up";
		public static string Down => "ui_down";

		public static string PageUp => "ui_page_up";
		public static string PageDown => "ui_page_down";

		public static string Home => "ui_home";
		public static string End => "ui_end";

		public static string TextCompletionAccept => "ui_text_completion_accept";
		public static string TextCompletionCancel => "ui_text_completion_cancel";
		public static string TextCompletionQuery => "ui_text_completion_query";

		public static string TextCaretLeft => "ui_text_caret_left";
		public static string TextCaretRight => "ui_text_caret_right";
		public static string TextCaretUp => "ui_text_caret_up";
		public static string TextCaretDown => "ui_text_caret_down";

		public static string TextCaretWordLeft => "ui_text_caret_word_left";
		public static string TextCaretWordRight => "ui_text_caret_word_right";
		public static string TextCaretPageUp => "ui_text_caret_page_up";
		public static string TextCaretPageDown => "ui_text_caret_page_down";

		public static string TextCaretLineStart => "ui_text_caret_line_start";
		public static string TextCaretLineEnd => "ui_text_caret_line_end";
		public static string TextCaretDocumentStart => "ui_text_caret_document_start";
		public static string TextCaretDocumentEnd => "ui_text_caret_document_end";

		public static string TextBackspace => "ui_text_backspace";
		public static string TextDelete => "ui_text_delete";
		public static string TextInsert => "ui_text_insert";

		public static string TextNewline => "ui_text_newline";
		public static string TextNewlineBlank => "ui_text_newline_blank";
		public static string TextNewlineAbove => "ui_text_newline_above";
		public static string TextNewlineBelow => "ui_text_newline_below";

		public static string TextIndent => "ui_text_indent";
		public static string TextDedent => "ui_text_dedent";

		public static string TextCompletionReplace => "ui_text_completion_replace";
		public static string TextCompletionSelectNext => "ui_text_completion_select_next";
		public static string TextCompletionSelectPrev => "ui_text_completion_select_prev";

		public static string TextScrollUp => "ui_text_scroll_up";
		public static string TextScrollDown => "ui_text_scroll_down";
		public static string TextScrollPageUp => "ui_text_scroll_page_up";
		public static string TextScrollPageDown => "ui_text_scroll_page_down";
		public static string TextScrollToCursor => "ui_text_scroll_to_cursor";

		public static string Menu => "ui_menu";
	}
}
