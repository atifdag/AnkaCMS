import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormControl } from '@angular/forms';
import { AddModel } from 'src/app/models/add-model';
import { Router } from '@angular/router';
import { MessageService, SelectItem } from 'src/app/primeng/components/common/api';
import { CategoryService } from '../category.service';
import { CategoryModel } from 'src/app/models/category-model';
import { LanguageService } from '../../language/language.service';
import { IdCodeName } from 'src/app/value-objects/id-code-name';
import { GlobalizationDictionaryPipe } from 'src/app/pipes/globalization-dictionary.pipe';
import { GlobalizationMessagesPipe } from 'src/app/pipes/globalization-messages.pipe';
import { AdminLayoutComponent } from 'src/app/layouts/admin/admin-layout/admin-layout.component';

@Component({
  selector: 'app-category-add',
  templateUrl: './category-add.component.html',
  styleUrls: ['./category-add.component.css']
})
export class CategoryAddComponent implements OnInit {
  disabledFieldset: boolean;
  model = new AddModel<CategoryModel>();
  userForm: FormGroup;
  languages: SelectItem[];
  isApprovedChecked: boolean;
  submitted: boolean;
  breadcrumbItems = [
    { label: this.globalizationDictionaryPipe.transform('HomePage'), routerLink: '/Home' },
    { label: this.globalizationDictionaryPipe.transform('CategoryManagement'), routerLink: '/Category/List' },
    { label: this.globalizationDictionaryPipe.transform('NewRecord') },
  ];

  constructor(
    private messageService: MessageService,
    private serviceLanguage: LanguageService,
    public globalizationDictionaryPipe: GlobalizationDictionaryPipe,
    private globalizationMessagesPipe: GlobalizationMessagesPipe,
    private serviceCategory: CategoryService,
    private adminLayoutComponent: AdminLayoutComponent,
    private router: Router
  ) { }

  globalizationMessagesByParameter(key: string, parameter: string): string {
    return this.globalizationMessagesPipe.transform(key + ',' + parameter);
  }

  globalizationMessagesByParameter2(key: string, parameter1: string, parameter2: string): string {
    return this.globalizationMessagesPipe.transform(key + ',' + parameter1 + ',' + parameter2);
  }

  ngOnInit() {
    this.adminLayoutComponent.resetCounter();
    this.model.item = new CategoryModel();

    this.userForm = new FormGroup({
      selectedLanguage: new FormControl('', Validators.required),
      code: new FormControl('', [Validators.required, Validators.minLength(2)]),
      name: new FormControl('', [Validators.required, Validators.minLength(2)]),
      description: new FormControl(''),
      keywords: new FormControl(''),
      isApproved: new FormControl(true)
    });


    this.serviceLanguage.keysAndValues().subscribe(
      r => {
        if (r.status === 200) {
          const list = r.body as IdCodeName[];
          if (list.length > 0) {
            this.languages = [];
            this.languages.push({ label: this.globalizationDictionaryPipe.transform('Select'), value: '' });
            list.forEach(x => {
              this.languages.push(
                { value: x.id, label: x.name }
              );
            });

            this.serviceCategory.beforeAdd().subscribe(
              res => {
                if (res.status === 200) {
                  this.model = res.body as AddModel<CategoryModel>;
                  if (this.model.item != null) {
                    this.userForm.get('code').setValue(this.model.item.code);
                    this.userForm.get('name').setValue(this.model.item.name);
                    this.userForm.get('description').setValue(this.model.item.description);
                    this.userForm.get('keywords').setValue(this.model.item.keywords);
                    this.userForm.get('isApproved').setValue(this.model.item.isApproved);
                  }
                  this.disabledFieldset = false;
                } else {
                  this.messageService.add({
                    severity: 'error',
                    summary: this.globalizationDictionaryPipe.transform('Error'),
                    detail: 'IN01 ' + res.statusText
                  });
                }
              },
              err => {
                if (err.status === 400) {
                  if (err.error != null) {
                    this.model.message = this.globalizationDictionaryPipe.transform('Error');
                    this.messageService.add({
                      severity: 'error',
                      summary: this.globalizationDictionaryPipe.transform('Error'),
                      detail: 'IN02 ' + this.model.message
                    });
                  } else {
                    this.model.message = err.error;

                    this.messageService.add({
                      severity: 'error',
                      summary: this.globalizationDictionaryPipe.transform('Error'),
                      detail: 'IN03 ' + this.model.message
                    });
                  }
                  setTimeout(() => {
                    this.router.navigate(['/Category/List']);
                  }, 1000);
                }
              }
            );
          } else {
            this.messageService.add({
              severity: 'error',
              summary: this.globalizationDictionaryPipe.transform('Error'),
              detail: this.globalizationMessagesPipe.transform('DangerParentNotFound')
            });
            setTimeout(() => {
              this.router.navigate(['/Category/List']);
            }, 1000);
          }
        } else {
          this.messageService.add({
            severity: 'error',
            summary: this.globalizationDictionaryPipe.transform('Error'),
            detail: 'LI05 ' + r.statusText
          });
        }
      },
      err => {
        if (err.status === 400) {
          if (err.error != null) {
            this.model.message = this.globalizationDictionaryPipe.transform('Error');
            this.messageService.add({
              severity: 'error',
              summary: this.globalizationDictionaryPipe.transform('Error'),
              detail: this.globalizationMessagesPipe.transform('DangerParentNotFound')
            });
            setTimeout(() => {
              this.router.navigate(['/Language/List']);
            }, 1000);
          } else {
            this.model.message = err.error;
            this.messageService.add({
              severity: 'error',
              summary: this.globalizationDictionaryPipe.transform('Error'),
              detail: 'LI07 ' + this.model.message
            });
          }
          setTimeout(() => {
            this.router.navigate(['/Category/List']);
          }, 1000);
        }
      }
    ); 
  }

  get f() {
    return this.userForm.controls;
  }

  onSubmit() {

    this.submitted = true;
    if (this.userForm.invalid) {
      return;
    }
    this.adminLayoutComponent.resetCounter();
    this.messageService.clear();

    if (this.model.item == null) {
      this.model.item = new CategoryModel();
    }
    if (this.model.item.language == null) {
      this.model.item.language = new IdCodeName();
    }
    this.model.item.language.id = this.f.selectedLanguage.value;
    this.model.item.code = this.f.code.value;
    this.model.item.name = this.f.name.value;
    this.model.item.description = this.f.description.value;
    this.model.item.keywords = this.f.keywords.value;
    this.model.item.isApproved = this.f.isApproved.value;
    this.serviceCategory.add(this.model).subscribe(
      res => {
        if (res.status === 200) {
          this.disabledFieldset = true;
          this.messageService.add({
            severity: 'success',
            summary: this.globalizationDictionaryPipe.transform('Success'),
            detail: this.globalizationMessagesPipe.transform('InfoSaveOperationSuccessful')
          });
          setTimeout(() => {
            this.router.navigate(['/Category/List']);
          }, 1000);
        } else {
          this.messageService.add({
            severity: 'error',
            summary: this.globalizationDictionaryPipe.transform('Error'),
            detail: res.statusText
          });
        }
      },
      err => {
        console.log(err);
        if (err.status === 400) {
          if (err.error != null) {
            this.model = err.error;
            const errors = Object.keys(err.error).map((t) => {
              return err.error[t];
            });
            this.messageService.add({
              severity: 'error',
              summary: this.globalizationDictionaryPipe.transform('Error'),
              detail: errors.toString()
            });
          } else {
            this.messageService.add({
              severity: 'error',
              summary: this.globalizationDictionaryPipe.transform('Error'),
              detail: err.error
            });
          }
        }
      }
    );
  }

  backClick() {
    this.router.navigate(['/Category/List']);
  }
}
