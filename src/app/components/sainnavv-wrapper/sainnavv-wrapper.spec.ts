import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SainnavvWrapper } from './sainnavv-wrapper';

describe('SainnavvWrapper', () => {
  let component: SainnavvWrapper;
  let fixture: ComponentFixture<SainnavvWrapper>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SainnavvWrapper]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SainnavvWrapper);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
